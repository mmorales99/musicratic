using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.RegisterDevice;

public sealed record RegisterDeviceCommand(
    Guid UserId,
    string Token,
    DevicePlatform Platform,
    string? DeviceName = null) : ICommand<Guid>;
