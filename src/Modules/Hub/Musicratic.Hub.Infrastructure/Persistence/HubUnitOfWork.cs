using Musicratic.Hub.Application;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class HubUnitOfWork : IHubUnitOfWork
{
    private readonly HubDbContext _dbContext;

    public HubUnitOfWork(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
