using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Infrastructure.Configuration;

namespace Musicratic.Auth.Infrastructure.Services;

public sealed class OidcDiscoveryService : IOidcDiscoveryService, IDisposable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthentikOptions _options;
    private readonly ILogger<OidcDiscoveryService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private OidcDiscoveryDocument? _cachedDocument;
    private DateTime _cacheExpiry = DateTime.MinValue;

    public OidcDiscoveryService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthentikOptions> options,
        ILogger<OidcDiscoveryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetAuthorizationEndpointAsync(CancellationToken cancellationToken = default)
    {
        var doc = await GetDiscoveryDocumentAsync(cancellationToken);
        return doc.AuthorizationEndpoint;
    }

    public async Task<string> GetTokenEndpointAsync(CancellationToken cancellationToken = default)
    {
        var doc = await GetDiscoveryDocumentAsync(cancellationToken);
        return doc.TokenEndpoint;
    }

    public async Task<string> GetUserInfoEndpointAsync(CancellationToken cancellationToken = default)
    {
        var doc = await GetDiscoveryDocumentAsync(cancellationToken);
        return doc.UserInfoEndpoint;
    }

    public async Task<string> GetEndSessionEndpointAsync(CancellationToken cancellationToken = default)
    {
        var doc = await GetDiscoveryDocumentAsync(cancellationToken);
        return doc.EndSessionEndpoint;
    }

    public async Task<string> GetJwksUriAsync(CancellationToken cancellationToken = default)
    {
        var doc = await GetDiscoveryDocumentAsync(cancellationToken);
        return doc.JwksUri;
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    private async Task<OidcDiscoveryDocument> GetDiscoveryDocumentAsync(
        CancellationToken cancellationToken)
    {
        if (_cachedDocument is not null && DateTime.UtcNow < _cacheExpiry)
            return _cachedDocument;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cachedDocument is not null && DateTime.UtcNow < _cacheExpiry)
                return _cachedDocument;

            var discoveryUrl = $"{_options.Authority.TrimEnd('/')}/.well-known/openid-configuration";

            _logger.LogInformation(
                "Fetching OIDC discovery document from {DiscoveryUrl}", discoveryUrl);

            var client = _httpClientFactory.CreateClient("Authentik");
            using var response = await client.GetAsync(discoveryUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var document = await JsonSerializer.DeserializeAsync<OidcDiscoveryDocument>(
                stream, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException(
                    "Failed to deserialize OIDC discovery document.");

            _cachedDocument = document;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

            _logger.LogInformation("OIDC discovery document cached successfully");
            return document;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch OIDC discovery document");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
