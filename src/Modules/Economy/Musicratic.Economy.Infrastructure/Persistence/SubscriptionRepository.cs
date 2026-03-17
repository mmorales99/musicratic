using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class SubscriptionRepository(EconomyDbContext dbContext) : ISubscriptionRepository
{
    public async Task<Subscription?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> Find(
        Expression<Func<Subscription, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(Subscription entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Subscriptions.AddAsync(entity, cancellationToken);
    }

    public void Update(Subscription entity)
    {
        dbContext.Subscriptions.Update(entity);
    }

    public void Remove(Subscription entity)
    {
        dbContext.Subscriptions.Remove(entity);
    }

    public async Task<Subscription?> GetByHubId(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.HubId == hubId, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetExpiredTrials(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        return await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.IsActive && s.TrialEndsAt.HasValue && s.TrialEndsAt < utcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveByUserId(
        Guid userId, CancellationToken cancellationToken = default)
    {
        // Note: This requires a join with hub ownership which would typically be
        // resolved via cross-module query. For now, returns count of active subscriptions.
        return await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .CountAsync(s => s.IsActive, cancellationToken);
    }
}
