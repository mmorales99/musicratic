using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Repositories;

public interface IDeviceTokenRepository : IRepository<DeviceToken>
{
    Task<IReadOnlyList<DeviceToken>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceToken>> GetByUserIdAndPlatform(
        Guid userId,
        DevicePlatform platform,
        CancellationToken cancellationToken = default);

    Task<DeviceToken?> GetByUserIdAndToken(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);
}
