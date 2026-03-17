namespace Musicratic.Analytics.Api.DTOs;

public sealed record TrackStatsResponse(
    Guid TrackId,
    Guid HubId,
    int Upvotes,
    int Downvotes,
    int Plays,
    int Skips,
    double TotalPlayDurationSeconds,
    DateTime? LastPlayedAt,
    double Score);
