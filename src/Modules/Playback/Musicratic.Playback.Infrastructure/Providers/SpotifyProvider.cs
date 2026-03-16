using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Models;
using Musicratic.Playback.Domain.Services;
using Musicratic.Playback.Infrastructure.Configuration;

namespace Musicratic.Playback.Infrastructure.Providers;

public sealed class SpotifyProvider : IMusicProviderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SpotifyOptions _options;
    private readonly ILogger<SpotifyProvider> _logger;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public MusicProvider Provider => MusicProvider.Spotify;

    public SpotifyProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<SpotifyOptions> options,
        ILogger<SpotifyProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            _logger.LogWarning(
                "Spotify ClientId or ClientSecret is not configured. " +
                "Spotify provider will return empty results");
        }
    }

    public async Task<IReadOnlyList<TrackSearchResult>> Search(
        string query, int limit = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await CreateAuthenticatedClient(cancellationToken);
            if (client is null)
                return [];

            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_options.BaseUrl}/search?q={encodedQuery}&type=track&limit={limit}";

            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<SpotifySearchResponse>(cancellationToken);

            if (result?.Tracks?.Items is null)
                return [];

            return result.Tracks.Items
                .Select(MapToSearchResult)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Spotify search failed for query '{Query}'", query);
            return [];
        }
    }

    public async Task<TrackMetadata?> GetMetadata(
        string externalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await CreateAuthenticatedClient(cancellationToken);
            if (client is null)
                return null;

            var url = $"{_options.BaseUrl}/tracks/{Uri.EscapeDataString(externalId)}";

            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var track = await response.Content
                .ReadFromJsonAsync<SpotifyTrack>(cancellationToken);

            if (track is null)
                return null;

            return MapToMetadata(track);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex, "Spotify GetMetadata failed for externalId '{ExternalId}'", externalId);
            return null;
        }
    }

    public Task<string?> GetPlaybackUrl(
        string externalId, CancellationToken cancellationToken = default)
    {
        // Spotify uses URI format: spotify:track:{id}
        if (string.IsNullOrWhiteSpace(externalId))
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>($"spotify:track:{externalId}");
    }

    private async Task<HttpClient?> CreateAuthenticatedClient(
        CancellationToken cancellationToken)
    {
        var token = await GetAccessToken(cancellationToken);
        if (token is null)
            return null;

        var client = _httpClientFactory.CreateClient("SpotifyApi");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<string?> GetAccessToken(CancellationToken cancellationToken)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            return await RefreshAccessToken(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<string?> RefreshAccessToken(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("SpotifyAuth");

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            var content = new FormUrlEncodedContent(
                [new KeyValuePair<string, string>("grant_type", "client_credentials")]);

            using var response = await client.PostAsync(
                _options.TokenUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content
                .ReadFromJsonAsync<SpotifyTokenResponse>(cancellationToken);

            if (tokenResponse is null)
                return null;

            _cachedToken = tokenResponse.AccessToken;
            // Refresh 60 seconds before actual expiry for safety
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            _logger.LogInformation("Spotify access token refreshed successfully");
            return _cachedToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to obtain Spotify access token");
            _cachedToken = null;
            return null;
        }
    }

    private static TrackSearchResult MapToSearchResult(SpotifyTrack track)
    {
        return new TrackSearchResult(
            ExternalId: track.Id,
            Title: track.Name,
            Artist: string.Join(", ", track.Artists.Select(a => a.Name)),
            Album: track.Album?.Name,
            DurationSeconds: track.DurationMs / 1000,
            AlbumArtUrl: track.Album?.Images.FirstOrDefault()?.Url);
    }

    private static TrackMetadata MapToMetadata(SpotifyTrack track)
    {
        return new TrackMetadata(
            ExternalId: track.Id,
            Title: track.Name,
            Artist: string.Join(", ", track.Artists.Select(a => a.Name)),
            Album: track.Album?.Name,
            DurationSeconds: track.DurationMs / 1000,
            AlbumArtUrl: track.Album?.Images.FirstOrDefault()?.Url,
            PreviewUrl: track.PreviewUrl);
    }
}

#region Spotify API Response Models

internal sealed record SpotifyTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

internal sealed record SpotifySearchResponse(
    [property: JsonPropertyName("tracks")] SpotifyTrackList? Tracks);

internal sealed record SpotifyTrackList(
    [property: JsonPropertyName("items")] List<SpotifyTrack> Items);

internal sealed record SpotifyTrack(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("artists")] List<SpotifyArtist> Artists,
    [property: JsonPropertyName("album")] SpotifyAlbum? Album,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    [property: JsonPropertyName("preview_url")] string? PreviewUrl);

internal sealed record SpotifyArtist(
    [property: JsonPropertyName("name")] string Name);

internal sealed record SpotifyAlbum(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("images")] List<SpotifyImage> Images);

internal sealed record SpotifyImage(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("height")] int? Height,
    [property: JsonPropertyName("width")] int? Width);

#endregion
