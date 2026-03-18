using Musicratic.Analytics.Application;

namespace Musicratic.Analytics.Infrastructure.Persistence;

/// <summary>
/// ANLT-003: Analytics module UnitOfWork implementation.
/// </summary>
public sealed class AnalyticsUnitOfWork(AnalyticsDbContext dbContext) : IAnalyticsUnitOfWork
{
    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
