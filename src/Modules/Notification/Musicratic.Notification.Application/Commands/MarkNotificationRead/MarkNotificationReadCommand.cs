using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(
    Guid NotificationId,
    Guid UserId) : ICommand;
