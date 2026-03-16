using Musicratic.Shared.Application;

namespace Musicratic.Auth.Infrastructure.Persistence;

public sealed class AuthUnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _dbContext;

    public AuthUnitOfWork(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
