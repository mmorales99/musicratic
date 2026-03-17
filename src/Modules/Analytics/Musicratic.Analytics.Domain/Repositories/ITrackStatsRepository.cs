using Musicratic.Analytics.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Analytics.Domain.Repositories;

/// <summary>
/// ANLT-002: Repository for TrackStats with GetOrCreate pattern.
/// </summary>
public interface ITrackStatsRepository : IRepository<TrackStats>
{
    Task<TrackStats?> GetByTrackAndHub(
        Guid trackId, Guid hubId, CancellationToken cancellationToken = default);

    Task<TrackStats> GetOrCreate(
        Guid trackId, Guid hubId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackStats>> GetByHub(
        Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackStats>> GetByHubPaged(
        Guid hubId, int skip, int take, string sortBy, bool descending,
        CancellationToken cancellationToken = default);

    Task<int> CountByHub(Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackStats>> GetTopByHub(
        Guid hubId, int count, CancellationToken cancellationToken = default);
}
