using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Body,
    string? DataJson = null) : ICommand<Guid>;
