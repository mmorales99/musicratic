using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Notification.Infrastructure.Persistence;

namespace Musicratic.Notification.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.Notification?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Notification>> Find(
        Expression<Func<Domain.Entities.Notification, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        Domain.Entities.Notification entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(entity, cancellationToken);
    }

    public void Update(Domain.Entities.Notification entity)
    {
        _dbContext.Notifications.Update(entity);
    }

    public void Remove(Domain.Entities.Notification entity)
    {
        _dbContext.Notifications.Remove(entity);
    }

    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Domain.Entities.Notification> Items, int TotalCount)> GetByUserPaginated(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCount(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .CountAsync(n => n.UserId == userId && n.ReadAt == null, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetUnreadByUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        notification?.MarkAsRead();
    }
}
