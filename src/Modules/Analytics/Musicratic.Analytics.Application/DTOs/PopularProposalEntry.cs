namespace Musicratic.Analytics.Application.DTOs;

/// <summary>
/// ANLT-007: A single entry in the monthly popular proposals report.
/// </summary>
public sealed record PopularProposalEntry(
    Guid TrackId,
    int Plays,
    int Upvotes,
    double Score);
