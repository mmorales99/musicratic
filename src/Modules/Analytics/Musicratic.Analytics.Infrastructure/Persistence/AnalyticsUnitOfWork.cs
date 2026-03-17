using Musicratic.Shared.Application;

namespace Musicratic.Analytics.Infrastructure.Persistence;

/// <summary>
/// ANLT-003: Analytics module UnitOfWork implementation.
/// </summary>
public sealed class AnalyticsUnitOfWork(AnalyticsDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
