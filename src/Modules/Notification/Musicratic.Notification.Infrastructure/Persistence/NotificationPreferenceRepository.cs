using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;

namespace Musicratic.Notification.Infrastructure.Persistence;

public sealed class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationPreferenceRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationPreference?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationPreference>> Find(
        Expression<Func<NotificationPreference, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        NotificationPreference entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationPreferences.AddAsync(entity, cancellationToken);
    }

    public void Update(NotificationPreference entity)
    {
        _dbContext.NotificationPreferences.Update(entity);
    }

    public void Remove(NotificationPreference entity)
    {
        _dbContext.NotificationPreferences.Remove(entity);
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationPreference>> GetByUserIdAndType(
        Guid userId,
        NotificationType notificationType,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationPreferences
            .Where(p => p.UserId == userId && p.NotificationType == notificationType)
            .ToListAsync(cancellationToken);
    }

    public async Task Upsert(
        NotificationPreference preference,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p =>
                p.UserId == preference.UserId &&
                p.NotificationType == preference.NotificationType &&
                p.Channel == preference.Channel &&
                p.TenantId == preference.TenantId,
                cancellationToken);

        if (existing is not null)
        {
            if (preference.IsEnabled)
                existing.Enable();
            else
                existing.Disable();

            _dbContext.NotificationPreferences.Update(existing);
        }
        else
        {
            await _dbContext.NotificationPreferences.AddAsync(preference, cancellationToken);
        }
    }
}
