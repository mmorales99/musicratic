using Musicratic.Notification.Domain.Entities;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetUserDevices;

public sealed record GetUserDevicesQuery(Guid UserId) : IQuery<IReadOnlyList<DeviceToken>>;
