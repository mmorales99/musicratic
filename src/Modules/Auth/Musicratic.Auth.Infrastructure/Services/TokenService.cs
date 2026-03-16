using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Auth.Application.DTOs;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Infrastructure.Configuration;

namespace Musicratic.Auth.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOidcDiscoveryService _discoveryService;
    private readonly AuthentikOptions _options;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IHttpClientFactory httpClientFactory,
        IOidcDiscoveryService discoveryService,
        IOptions<AuthentikOptions> options,
        ILogger<TokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _discoveryService = discoveryService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TokenResponseDto> RefreshToken(
        string refreshToken, CancellationToken cancellationToken)
    {
        var tokenEndpoint = await _discoveryService.GetTokenEndpointAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient("Authentik");

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        using var content = new FormUrlEncodedContent(parameters);
        using var response = await client.PostAsync(tokenEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Token refresh failed with status {StatusCode}: {ErrorBody}",
                (int)response.StatusCode, errorBody);

            throw new TokenRefreshException(
                (int)response.StatusCode,
                "Failed to refresh token with the identity provider.");
        }

        var tokenResponse = await response.Content
            .ReadFromJsonAsync<AuthentikTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize token response.");

        return new TokenResponseDto(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            tokenResponse.ExpiresIn);
    }

    public async Task RevokeToken(string token, CancellationToken cancellationToken)
    {
        var revocationEndpoint = await _discoveryService
            .GetRevocationEndpointAsync(cancellationToken);

        var client = _httpClientFactory.CreateClient("Authentik");

        var parameters = new Dictionary<string, string>
        {
            ["token"] = token,
            ["token_type_hint"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        using var content = new FormUrlEncodedContent(parameters);
        using var response = await client.PostAsync(revocationEndpoint, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Token revocation failed with status {StatusCode}", (int)response.StatusCode);
        }
    }

    private sealed record AuthentikTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}

public sealed class TokenRefreshException : Exception
{
    public int StatusCode { get; }

    public TokenRefreshException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
