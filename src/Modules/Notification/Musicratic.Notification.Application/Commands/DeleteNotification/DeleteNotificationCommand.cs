using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.DeleteNotification;

public sealed record DeleteNotificationCommand(
    Guid NotificationId,
    Guid UserId) : ICommand;
