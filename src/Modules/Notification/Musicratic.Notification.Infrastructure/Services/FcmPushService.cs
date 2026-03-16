using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Notification.Application.Models;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Infrastructure.Configuration;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class FcmPushService : IFcmPushService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FcmOptions _options;
    private readonly ILogger<FcmPushService> _logger;

    private string? _cachedAccessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public FcmPushService(
        IHttpClientFactory httpClientFactory,
        IOptions<FcmOptions> options,
        ILogger<FcmPushService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;

        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "FCM configuration is incomplete. " +
                "Push notifications to Android devices will be skipped");
        }
    }

    public async Task<PushResult> SendPush(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogDebug("FCM not configured, skipping push to {DeviceToken}", deviceToken[..8]);
            return new PushResult(false, "FCM not configured");
        }

        try
        {
            var accessToken = await GetOrRefreshAccessToken(cancellationToken);
            var client = _httpClientFactory.CreateClient("FcmPush");

            var url = $"https://fcm.googleapis.com/v1/projects/{_options.ProjectId}/messages:send";
            var payload = BuildPayload(deviceToken, title, body, data);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
                return new PushResult(true);

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;

            _logger.LogWarning(
                "FCM push failed. Status: {StatusCode}, Body: {ErrorBody}, Token: {Token}",
                statusCode,
                errorBody,
                deviceToken[..Math.Min(8, deviceToken.Length)]);

            // UNREGISTERED or INVALID_ARGUMENT for bad tokens
            var shouldRemove =
                errorBody.Contains("UNREGISTERED", StringComparison.OrdinalIgnoreCase) ||
                errorBody.Contains("INVALID_ARGUMENT", StringComparison.OrdinalIgnoreCase);

            return new PushResult(false, $"FCM error {statusCode}: {errorBody}", shouldRemove);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM push failed unexpectedly for token {Token}",
                deviceToken[..Math.Min(8, deviceToken.Length)]);
            return new PushResult(false, ex.Message);
        }
    }

    private static object BuildPayload(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data)
    {
        var message = new Dictionary<string, object>
        {
            ["token"] = deviceToken,
            ["notification"] = new { title, body }
        };

        if (data is { Count: > 0 })
        {
            message["data"] = data;
        }

        return new { message };
    }

    private async Task<string> GetOrRefreshAccessToken(CancellationToken cancellationToken)
    {
        if (_cachedAccessToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedAccessToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedAccessToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedAccessToken;

            _cachedAccessToken = await RequestAccessToken(cancellationToken);
            // Google OAuth2 tokens are valid for 3600 seconds; refresh at 50 min
            _tokenExpiry = DateTime.UtcNow.AddMinutes(50);
            return _cachedAccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string> RequestAccessToken(CancellationToken cancellationToken)
    {
        var serviceAccount = JsonSerializer.Deserialize<ServiceAccountKey>(_options.ServiceAccountJson!);

        if (serviceAccount?.ClientEmail is null || serviceAccount.PrivateKey is null)
            throw new InvalidOperationException("Invalid FCM service account JSON");

        var now = DateTimeOffset.UtcNow;
        var jwt = CreateServiceAccountJwt(
            serviceAccount.ClientEmail,
            serviceAccount.PrivateKey,
            now);

        var client = _httpClientFactory.CreateClient("FcmAuth");
        var tokenRequest = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
            new KeyValuePair<string, string>("assertion", jwt)
        ]);

        using var response = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            tokenRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        return tokenResponse?.AccessToken
            ?? throw new InvalidOperationException("Failed to obtain FCM access token");
    }

    private static string CreateServiceAccountJwt(
        string clientEmail,
        string privateKey,
        DateTimeOffset now)
    {
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "RS256",
            typ = "JWT"
        }));

        var claims = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = clientEmail,
            scope = "https://www.googleapis.com/auth/firebase.messaging",
            aud = "https://oauth2.googleapis.com/token",
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddHours(1).ToUnixTimeSeconds()
        }));

        var unsignedToken = $"{header}.{claims}";
        var signature = SignWithRsa(Encoding.UTF8.GetBytes(unsignedToken), privateKey);

        return $"{unsignedToken}.{signature}";
    }

    private static string SignWithRsa(byte[] data, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        var keyBytes = Convert.FromBase64String(
            privateKeyPem
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim());

        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        var signed = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Base64UrlEncode(signed);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class ServiceAccountKey
    {
        [JsonPropertyName("client_email")]
        public string? ClientEmail { get; set; }

        [JsonPropertyName("private_key")]
        public string? PrivateKey { get; set; }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
