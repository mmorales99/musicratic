using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Notification.Infrastructure.Persistence;

namespace Musicratic.Notification.Infrastructure.Persistence;

public sealed class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly NotificationDbContext _dbContext;

    public DeviceTokenRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DeviceToken?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> Find(
        Expression<Func<DeviceToken, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        DeviceToken entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DeviceTokens.AddAsync(entity, cancellationToken);
    }

    public void Update(DeviceToken entity)
    {
        _dbContext.DeviceTokens.Update(entity);
    }

    public void Remove(DeviceToken entity)
    {
        _dbContext.DeviceTokens.Remove(entity);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .Where(d => d.UserId == userId && d.IsActive)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceToken>> GetByUserIdAndPlatform(
        Guid userId,
        DevicePlatform platform,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .Where(d => d.UserId == userId && d.Platform == platform && d.IsActive)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceToken?> GetByUserIdAndToken(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceTokens
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Token == token, cancellationToken);
    }
}
