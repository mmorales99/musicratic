using Microsoft.Extensions.Configuration;
using Musicratic.Social.Application.Services;

namespace Musicratic.Social.Infrastructure.Services;

public sealed class SocialSharingService : ISocialSharingService
{
    private const string DefaultBaseUrl = "https://musicratic.app";
    private readonly string _baseUrl;

    public SocialSharingService(IConfiguration configuration)
    {
        _baseUrl = configuration["Musicratic:BaseUrl"] ?? DefaultBaseUrl;
    }

    public ShareLink GenerateHubShareLink(Guid hubId, string hubName, string? description)
    {
        var url = $"{_baseUrl}/hub/{hubId}";
        var title = $"{hubName} on Musicratic";
        var desc = description ?? $"Listen and vote on {hubName} — collaborative music playback.";

        return new ShareLink(url, title, desc, ImageUrl: null);
    }

    public ShareLink GenerateTrackShareLink(
        Guid hubId, string hubName, string trackTitle, string artistName)
    {
        var url = $"{_baseUrl}/hub/{hubId}";
        var title = $"{trackTitle} by {artistName} — playing on {hubName}";
        var desc = $"Now playing on {hubName}. Join and vote on Musicratic!";

        return new ShareLink(url, title, desc, ImageUrl: null);
    }

    public ShareLink GenerateListShareLink(Guid listId, string listName, string curatorName)
    {
        var url = $"{_baseUrl}/lists/{listId}";
        var title = $"{listName} — curated by {curatorName}";
        var desc = $"Check out \"{listName}\" on Musicratic.";

        return new ShareLink(url, title, desc, ImageUrl: null);
    }

    public ShareLink GenerateProfileShareLink(Guid userId, string displayName)
    {
        var url = $"{_baseUrl}/profile/{userId}";
        var title = $"{displayName} on Musicratic";
        var desc = $"See {displayName}'s profile, public lists and stats.";

        return new ShareLink(url, title, desc, ImageUrl: null);
    }
}
