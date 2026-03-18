using Musicratic.Economy.Application;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class EconomyUnitOfWork(EconomyDbContext dbContext) : IEconomyUnitOfWork
{
    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
