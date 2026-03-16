using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Notification.Application.Models;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Infrastructure.Configuration;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class ApnsPushService : IApnsPushService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApnsOptions _options;
    private readonly ILogger<ApnsPushService> _logger;

    private string? _cachedJwt;
    private DateTime _jwtExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _jwtLock = new(1, 1);

    public ApnsPushService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApnsOptions> options,
        ILogger<ApnsPushService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;

        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "APNs configuration is incomplete. " +
                "Push notifications to iOS devices will be skipped");
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
            _logger.LogDebug("APNs not configured, skipping push to {DeviceToken}", deviceToken[..8]);
            return new PushResult(false, "APNs not configured");
        }

        try
        {
            var jwt = await GetOrRefreshJwt(cancellationToken);
            var client = _httpClientFactory.CreateClient("ApnsPush");

            var url = $"{_options.BaseUrl}/3/device/{deviceToken}";
            var payload = BuildPayload(title, body, data);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", jwt);
            request.Headers.TryAddWithoutValidation("apns-topic", _options.BundleId);
            request.Headers.TryAddWithoutValidation("apns-push-type", "alert");
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
                "APNs push failed. Status: {StatusCode}, Body: {ErrorBody}, Token: {Token}",
                statusCode,
                errorBody,
                deviceToken[..Math.Min(8, deviceToken.Length)]);

            // 410 Gone = device unregistered; 400 with BadDeviceToken = invalid token
            var shouldRemove = statusCode == 410 ||
                               errorBody.Contains("BadDeviceToken", StringComparison.OrdinalIgnoreCase) ||
                               errorBody.Contains("Unregistered", StringComparison.OrdinalIgnoreCase);

            return new PushResult(false, $"APNs error {statusCode}: {errorBody}", shouldRemove);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "APNs push failed unexpectedly for token {Token}",
                deviceToken[..Math.Min(8, deviceToken.Length)]);
            return new PushResult(false, ex.Message);
        }
    }

    private static object BuildPayload(
        string title,
        string body,
        Dictionary<string, string>? data)
    {
        var payload = new Dictionary<string, object>
        {
            ["aps"] = new Dictionary<string, object>
            {
                ["alert"] = new { title, body },
                ["sound"] = "default"
            }
        };

        if (data is not null)
        {
            foreach (var kvp in data)
            {
                payload[kvp.Key] = kvp.Value;
            }
        }

        return payload;
    }

    private async Task<string> GetOrRefreshJwt(CancellationToken cancellationToken)
    {
        if (_cachedJwt is not null && DateTime.UtcNow < _jwtExpiry)
            return _cachedJwt;

        await _jwtLock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedJwt is not null && DateTime.UtcNow < _jwtExpiry)
                return _cachedJwt;

            _cachedJwt = GenerateJwt();
            // APNs tokens are valid for 60 min; refresh at 50 min
            _jwtExpiry = DateTime.UtcNow.AddMinutes(50);
            return _cachedJwt;
        }
        finally
        {
            _jwtLock.Release();
        }
    }

    private string GenerateJwt()
    {
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "ES256",
            kid = _options.KeyId
        }));

        var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claims = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = _options.TeamId,
            iat = issuedAt
        }));

        var unsignedToken = $"{header}.{claims}";
        var signature = SignWithECDsa(Encoding.UTF8.GetBytes(unsignedToken));

        return $"{unsignedToken}.{signature}";
    }

    private string SignWithECDsa(byte[] data)
    {
        using var ecdsa = ECDsa.Create();
        var keyBytes = Convert.FromBase64String(
            _options.P8PrivateKey!
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim());

        ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);
        var signed = ecdsa.SignData(data, HashAlgorithmName.SHA256);
        return Base64UrlEncode(signed);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
