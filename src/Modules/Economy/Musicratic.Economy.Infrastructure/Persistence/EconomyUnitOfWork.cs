using Musicratic.Shared.Application;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class EconomyUnitOfWork(EconomyDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
