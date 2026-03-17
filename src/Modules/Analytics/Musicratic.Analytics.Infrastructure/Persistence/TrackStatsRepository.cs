using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Analytics.Domain.Entities;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Infrastructure.Persistence;

/// <summary>
/// ANLT-002: Repository implementation for TrackStats with GetOrCreate pattern.
/// </summary>
public sealed class TrackStatsRepository(AnalyticsDbContext dbContext) : ITrackStatsRepository
{
    public async Task<TrackStats?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrackStats>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrackStats>> Find(
        Expression<Func<TrackStats, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(TrackStats entity, CancellationToken cancellationToken = default)
    {
        await dbContext.TrackStats.AddAsync(entity, cancellationToken);
    }

    public void Update(TrackStats entity)
    {
        dbContext.TrackStats.Update(entity);
    }

    public void Remove(TrackStats entity)
    {
        dbContext.TrackStats.Remove(entity);
    }

    public async Task<TrackStats?> GetByTrackAndHub(
        Guid trackId, Guid hubId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats
            .FirstOrDefaultAsync(
                t => t.TrackId == trackId && t.HubId == hubId,
                cancellationToken);
    }

    public async Task<TrackStats> GetOrCreate(
        Guid trackId, Guid hubId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TrackStats
            .FirstOrDefaultAsync(
                t => t.TrackId == trackId && t.HubId == hubId,
                cancellationToken);

        if (existing is not null)
            return existing;

        var stats = TrackStats.Create(trackId, hubId, tenantId);
        await dbContext.TrackStats.AddAsync(stats, cancellationToken);
        return stats;
    }

    public async Task<IReadOnlyList<TrackStats>> GetByHub(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats
            .Where(t => t.HubId == hubId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrackStats>> GetByHubPaged(
        Guid hubId, int skip, int take, string sortBy, bool descending,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.TrackStats.Where(t => t.HubId == hubId);

        query = sortBy?.ToLowerInvariant() switch
        {
            "plays" => descending ? query.OrderByDescending(t => t.Plays) : query.OrderBy(t => t.Plays),
            "upvotes" => descending ? query.OrderByDescending(t => t.Upvotes) : query.OrderBy(t => t.Upvotes),
            "downvotes" => descending ? query.OrderByDescending(t => t.Downvotes) : query.OrderBy(t => t.Downvotes),
            "skips" => descending ? query.OrderByDescending(t => t.Skips) : query.OrderBy(t => t.Skips),
            "last_played" => descending ? query.OrderByDescending(t => t.LastPlayedAt) : query.OrderBy(t => t.LastPlayedAt),
            _ => descending ? query.OrderByDescending(t => t.Plays) : query.OrderBy(t => t.Plays)
        };

        return await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<int> CountByHub(Guid hubId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats.CountAsync(t => t.HubId == hubId, cancellationToken);
    }

    public async Task<IReadOnlyList<TrackStats>> GetTopByHub(
        Guid hubId, int count, CancellationToken cancellationToken = default)
    {
        return await dbContext.TrackStats
            .Where(t => t.HubId == hubId && (t.Upvotes + t.Downvotes) > 0)
            .OrderByDescending(t => t.Upvotes - t.Downvotes)
            .ThenByDescending(t => t.Plays)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
