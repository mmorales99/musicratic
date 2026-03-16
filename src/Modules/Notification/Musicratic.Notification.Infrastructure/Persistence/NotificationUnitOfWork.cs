using Musicratic.Shared.Application;

namespace Musicratic.Notification.Infrastructure.Persistence;

public sealed class NotificationUnitOfWork : IUnitOfWork
{
    private readonly NotificationDbContext _dbContext;

    public NotificationUnitOfWork(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
