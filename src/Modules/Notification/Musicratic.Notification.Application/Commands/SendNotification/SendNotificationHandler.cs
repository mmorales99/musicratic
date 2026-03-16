using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.SendNotification;

public sealed class SendNotificationHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    INotificationPushService webSocketPushService,
    IPushNotificationService pushNotificationService) : ICommandHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = Domain.Entities.Notification.Create(
            request.UserId,
            request.Type,
            request.Title,
            request.Body,
            request.DataJson);

        await notificationRepository.Add(notification, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        // WebSocket delivery
        await webSocketPushService.SendToUser(request.UserId, notification, cancellationToken);

        // Mobile push notification delivery (failures are handled inside the service)
        var data = request.DataJson is not null
            ? new Dictionary<string, string> { ["json"] = request.DataJson }
            : null;

        await pushNotificationService.SendToUser(
            request.UserId,
            request.Title,
            request.Body,
            data,
            cancellationToken);

        return notification.Id;
    }
}
