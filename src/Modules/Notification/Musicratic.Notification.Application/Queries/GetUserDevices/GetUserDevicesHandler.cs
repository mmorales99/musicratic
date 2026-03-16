using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetUserDevices;

public sealed class GetUserDevicesHandler(
    IDeviceTokenRepository deviceTokenRepository) : IQueryHandler<GetUserDevicesQuery, IReadOnlyList<DeviceToken>>
{
    public async Task<IReadOnlyList<DeviceToken>> Handle(
        GetUserDevicesQuery request,
        CancellationToken cancellationToken)
    {
        return await deviceTokenRepository.GetByUserId(request.UserId, cancellationToken);
    }
}
