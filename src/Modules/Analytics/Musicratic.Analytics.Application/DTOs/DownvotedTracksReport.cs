namespace Musicratic.Analytics.Application.DTOs;

/// <summary>
/// ANLT-006: Weekly report of tracks with >40% downvotes in a hub.
/// </summary>
public sealed record DownvotedTracksReport(
    Guid HubId,
    DateTime GeneratedAt,
    IReadOnlyList<DownvotedTrackEntry> Tracks);
