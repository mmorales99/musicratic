using Musicratic.Analytics.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-004: Aggregates analytics counters using GetOrCreate + increment pattern.
/// </summary>
public sealed class StatsAggregationService(
    ITrackStatsRepository trackStatsRepository,
    IHubStatsRepository hubStatsRepository,
    IAnalyticsUnitOfWork unitOfWork) : IStatsAggregationService
{
    public async Task RecordTrackPlay(
        Guid trackId, Guid hubId, Guid tenantId, TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var trackStats = await trackStatsRepository.GetOrCreate(
            trackId, hubId, tenantId, cancellationToken);

        trackStats.IncrementPlays(duration);
        trackStatsRepository.Update(trackStats);

        var hubStats = await hubStatsRepository.GetOrCreate(
            hubId, tenantId, cancellationToken);

        hubStats.IncrementPlays();
        hubStatsRepository.Update(hubStats);

        await unitOfWork.SaveChanges(cancellationToken);
    }

    public async Task RecordVote(
        Guid trackId, Guid hubId, Guid tenantId, bool isUpvote,
        CancellationToken cancellationToken = default)
    {
        var trackStats = await trackStatsRepository.GetOrCreate(
            trackId, hubId, tenantId, cancellationToken);

        if (isUpvote)
            trackStats.IncrementUpvotes();
        else
            trackStats.IncrementDownvotes();

        trackStatsRepository.Update(trackStats);

        var hubStats = await hubStatsRepository.GetOrCreate(
            hubId, tenantId, cancellationToken);

        hubStats.IncrementVotes();
        hubStatsRepository.Update(hubStats);

        await unitOfWork.SaveChanges(cancellationToken);
    }

    public async Task RecordSkip(
        Guid trackId, Guid hubId, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var trackStats = await trackStatsRepository.GetOrCreate(
            trackId, hubId, tenantId, cancellationToken);

        trackStats.IncrementSkips();
        trackStatsRepository.Update(trackStats);

        await unitOfWork.SaveChanges(cancellationToken);
    }

    public async Task RecordHubActivity(
        Guid hubId, Guid tenantId, int concurrentUsers,
        CancellationToken cancellationToken = default)
    {
        var hubStats = await hubStatsRepository.GetOrCreate(
            hubId, tenantId, cancellationToken);

        hubStats.UpdateConcurrentUsers(concurrentUsers);
        hubStatsRepository.Update(hubStats);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
