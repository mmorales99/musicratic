using Musicratic.Shared.Application;

namespace Musicratic.Social.Infrastructure.Persistence;

public sealed class SocialUnitOfWork : IUnitOfWork
{
    private readonly SocialDbContext _dbContext;

    public SocialUnitOfWork(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
