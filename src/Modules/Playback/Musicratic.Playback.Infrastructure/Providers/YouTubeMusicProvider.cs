using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Models;
using Musicratic.Playback.Domain.Services;
using Musicratic.Playback.Infrastructure.Configuration;

namespace Musicratic.Playback.Infrastructure.Providers;

public sealed class YouTubeMusicProvider : IMusicProviderService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly YouTubeOptions _options;
    private readonly ILogger<YouTubeMusicProvider> _logger;

    public MusicProvider Provider => MusicProvider.YoutubeMusic;

    public YouTubeMusicProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<YouTubeOptions> options,
        ILogger<YouTubeMusicProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "YouTube ApiKey is not configured. " +
                "YouTube provider will return empty results");
        }
    }

    public async Task<IReadOnlyList<TrackSearchResult>> Search(
        string query, int limit = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return [];

        try
        {
            var client = _httpClientFactory.CreateClient("YouTubeApi");
            var encodedQuery = Uri.EscapeDataString(query);

            var searchUrl =
                $"{_options.BaseUrl}/search?part=snippet&type=video" +
                $"&videoCategoryId=10&maxResults={limit}" +
                $"&q={encodedQuery}&key={_options.ApiKey}";

            using var searchResponse = await client.GetAsync(searchUrl, cancellationToken);
            searchResponse.EnsureSuccessStatusCode();

            var searchResult = await searchResponse.Content
                .ReadFromJsonAsync<YouTubeSearchResponse>(cancellationToken);

            if (searchResult?.Items is null or { Count: 0 })
                return [];

            var videoIds = string.Join(",",
                searchResult.Items.Select(i => i.Id.VideoId));

            return await GetVideoDetails(client, videoIds, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "YouTube search failed for query '{Query}'", query);
            return [];
        }
    }

    public async Task<TrackMetadata?> GetMetadata(
        string externalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient("YouTubeApi");
            var escapedId = Uri.EscapeDataString(externalId);

            var url =
                $"{_options.BaseUrl}/videos?part=snippet,contentDetails" +
                $"&id={escapedId}&key={_options.ApiKey}";

            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<YouTubeVideoListResponse>(cancellationToken);

            var video = result?.Items?.FirstOrDefault();
            if (video is null)
                return null;

            return new TrackMetadata(
                ExternalId: externalId,
                Title: video.Snippet.Title,
                Artist: video.Snippet.ChannelTitle,
                Album: null,
                DurationSeconds: ParseIsoDuration(video.ContentDetails.Duration),
                AlbumArtUrl: GetBestThumbnail(video.Snippet.Thumbnails),
                PreviewUrl: null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex, "YouTube GetMetadata failed for externalId '{ExternalId}'", externalId);
            return null;
        }
    }

    public Task<string?> GetPlaybackUrl(
        string externalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>($"https://www.youtube.com/watch?v={externalId}");
    }

    private async Task<IReadOnlyList<TrackSearchResult>> GetVideoDetails(
        HttpClient client, string videoIds, CancellationToken cancellationToken)
    {
        var url =
            $"{_options.BaseUrl}/videos?part=snippet,contentDetails" +
            $"&id={videoIds}&key={_options.ApiKey}";

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<YouTubeVideoListResponse>(cancellationToken);

        if (result?.Items is null)
            return [];

        return result.Items
            .Select(v => new TrackSearchResult(
                ExternalId: v.Id,
                Title: v.Snippet.Title,
                Artist: v.Snippet.ChannelTitle,
                Album: null,
                DurationSeconds: ParseIsoDuration(v.ContentDetails.Duration),
                AlbumArtUrl: GetBestThumbnail(v.Snippet.Thumbnails)))
            .ToList();
    }

    /// <summary>
    /// Parses ISO 8601 duration (e.g., PT4M13S) to total seconds.
    /// </summary>
    internal static int ParseIsoDuration(string isoDuration)
    {
        try
        {
            var duration = XmlConvert.ToTimeSpan(isoDuration);
            return (int)duration.TotalSeconds;
        }
        catch
        {
            return 0;
        }
    }

    private static string? GetBestThumbnail(YouTubeThumbnails? thumbnails)
    {
        if (thumbnails is null)
            return null;

        return thumbnails.High?.Url
            ?? thumbnails.Medium?.Url
            ?? thumbnails.Default?.Url;
    }
}

#region YouTube API Response Models

internal sealed record YouTubeSearchResponse(
    [property: JsonPropertyName("items")] List<YouTubeSearchItem>? Items);

internal sealed record YouTubeSearchItem(
    [property: JsonPropertyName("id")] YouTubeSearchItemId Id);

internal sealed record YouTubeSearchItemId(
    [property: JsonPropertyName("videoId")] string VideoId);

internal sealed record YouTubeVideoListResponse(
    [property: JsonPropertyName("items")] List<YouTubeVideo>? Items);

internal sealed record YouTubeVideo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("snippet")] YouTubeSnippet Snippet,
    [property: JsonPropertyName("contentDetails")] YouTubeContentDetails ContentDetails);

internal sealed record YouTubeSnippet(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("channelTitle")] string ChannelTitle,
    [property: JsonPropertyName("thumbnails")] YouTubeThumbnails? Thumbnails);

internal sealed record YouTubeContentDetails(
    [property: JsonPropertyName("duration")] string Duration);

internal sealed record YouTubeThumbnails(
    [property: JsonPropertyName("default")] YouTubeThumbnail? Default,
    [property: JsonPropertyName("medium")] YouTubeThumbnail? Medium,
    [property: JsonPropertyName("high")] YouTubeThumbnail? High);

internal sealed record YouTubeThumbnail(
    [property: JsonPropertyName("url")] string Url);

#endregion
