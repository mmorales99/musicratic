using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.UnregisterDevice;

public sealed record UnregisterDeviceCommand(
    Guid UserId,
    Guid TokenId) : ICommand;
