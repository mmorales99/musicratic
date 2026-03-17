namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-004: Service for recording and aggregating analytics events.
/// Uses upsert pattern (GetOrCreate then increment) on repositories.
/// </summary>
public interface IStatsAggregationService
{
    Task RecordTrackPlay(
        Guid trackId, Guid hubId, Guid tenantId, TimeSpan duration,
        CancellationToken cancellationToken = default);

    Task RecordVote(
        Guid trackId, Guid hubId, Guid tenantId, bool isUpvote,
        CancellationToken cancellationToken = default);

    Task RecordSkip(
        Guid trackId, Guid hubId, Guid tenantId,
        CancellationToken cancellationToken = default);

    Task RecordHubActivity(
        Guid hubId, Guid tenantId, int concurrentUsers,
        CancellationToken cancellationToken = default);
}
