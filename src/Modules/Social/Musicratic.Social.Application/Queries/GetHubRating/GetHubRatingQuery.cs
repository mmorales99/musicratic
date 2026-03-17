using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Queries.GetHubRating;

public sealed record GetHubRatingQuery(Guid HubId) : IQuery<HubRatingDto>;

/// <summary>
/// IsPublic = true only if reviewCount >= 3 (per docs/09-social-features.md §3).
/// </summary>
public sealed record HubRatingDto(
    Guid HubId,
    double AverageRating,
    int ReviewCount,
    bool IsPublic);
