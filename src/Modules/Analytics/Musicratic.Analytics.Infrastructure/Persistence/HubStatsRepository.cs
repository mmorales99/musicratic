using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Analytics.Domain.Entities;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Infrastructure.Persistence;

/// <summary>
/// ANLT-002: Repository implementation for HubStats with GetOrCreate pattern.
/// </summary>
public sealed class HubStatsRepository(AnalyticsDbContext dbContext) : IHubStatsRepository
{
    public async Task<HubStats?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.HubStats
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HubStats>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.HubStats.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HubStats>> Find(
        Expression<Func<HubStats, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.HubStats.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(HubStats entity, CancellationToken cancellationToken = default)
    {
        await dbContext.HubStats.AddAsync(entity, cancellationToken);
    }

    public void Update(HubStats entity)
    {
        dbContext.HubStats.Update(entity);
    }

    public void Remove(HubStats entity)
    {
        dbContext.HubStats.Remove(entity);
    }

    public async Task<HubStats?> GetByHub(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        return await dbContext.HubStats
            .FirstOrDefaultAsync(h => h.HubId == hubId, cancellationToken);
    }

    public async Task<HubStats> GetOrCreate(
        Guid hubId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.HubStats
            .FirstOrDefaultAsync(h => h.HubId == hubId, cancellationToken);

        if (existing is not null)
            return existing;

        var stats = HubStats.Create(hubId, tenantId);
        await dbContext.HubStats.AddAsync(stats, cancellationToken);
        return stats;
    }

    public async Task<IReadOnlyList<HubStats>> GetActiveHubs(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.HubStats
            .Where(h => h.LastActivityAt != null)
            .ToListAsync(cancellationToken);
    }
}
