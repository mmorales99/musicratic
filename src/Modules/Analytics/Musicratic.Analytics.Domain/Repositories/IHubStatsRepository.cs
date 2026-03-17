using Musicratic.Analytics.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Analytics.Domain.Repositories;

/// <summary>
/// ANLT-002: Repository for HubStats with GetOrCreate pattern.
/// </summary>
public interface IHubStatsRepository : IRepository<HubStats>
{
    Task<HubStats?> GetByHub(
        Guid hubId, CancellationToken cancellationToken = default);

    Task<HubStats> GetOrCreate(
        Guid hubId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubStats>> GetActiveHubs(
        CancellationToken cancellationToken = default);
}
