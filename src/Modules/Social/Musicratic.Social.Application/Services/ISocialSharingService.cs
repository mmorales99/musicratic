namespace Musicratic.Social.Application.Services;

public interface ISocialSharingService
{
    ShareLink GenerateHubShareLink(Guid hubId, string hubName, string? description);

    ShareLink GenerateTrackShareLink(Guid hubId, string hubName, string trackTitle, string artistName);

    ShareLink GenerateListShareLink(Guid listId, string listName, string curatorName);

    ShareLink GenerateProfileShareLink(Guid userId, string displayName);
}

public sealed record ShareLink(string Url, string Title, string Description, string? ImageUrl);
