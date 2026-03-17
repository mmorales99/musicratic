namespace Musicratic.Analytics.Application.DTOs;

/// <summary>
/// ANLT-006: A single entry in the weekly downvoted tracks report.
/// </summary>
public sealed record DownvotedTrackEntry(
    Guid TrackId,
    int Upvotes,
    int Downvotes,
    double DownvotePercentage,
    int Plays);
