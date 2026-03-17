using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.SendNotification;

public sealed class SendNotificationHandler(
    INotificationRepository notificationRepository,
    INotificationPreferenceRepository preferenceRepository,
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

        var preferences = await preferenceRepository.GetByUserIdAndType(
            request.UserId, request.Type, cancellationToken);

        // WebSocket delivery — check InApp preference (enabled by default)
        if (IsChannelEnabled(preferences, NotificationChannel.InApp))
        {
            await webSocketPushService.SendToUser(request.UserId, notification, cancellationToken);
        }

        // Mobile push notification delivery — check Push preference (enabled by default)
        if (IsChannelEnabled(preferences, NotificationChannel.Push))
        {
            var data = request.DataJson is not null
                ? new Dictionary<string, string> { ["json"] = request.DataJson }
                : null;

            await pushNotificationService.SendToUser(
                request.UserId,
                request.Title,
                request.Body,
                data,
                cancellationToken);
        }

        return notification.Id;
    }

    private static bool IsChannelEnabled(
        IReadOnlyList<Domain.Entities.NotificationPreference> preferences,
        NotificationChannel channel)
    {
        var pref = preferences.FirstOrDefault(p => p.Channel == channel);
        return pref?.IsEnabled ?? true;
    }
}
