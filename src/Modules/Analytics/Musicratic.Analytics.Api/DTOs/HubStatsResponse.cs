namespace Musicratic.Analytics.Api.DTOs;

public sealed record HubStatsResponse(
    Guid HubId,
    int TotalPlays,
    int TotalVotes,
    int UniqueListeners,
    double ActiveHours,
    int PeakConcurrentUsers,
    DateTime? LastActivityAt);
