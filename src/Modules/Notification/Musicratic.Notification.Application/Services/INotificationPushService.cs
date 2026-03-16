namespace Musicratic.Notification.Application.Services;

public interface INotificationPushService
{
    Task SendToUser(
        Guid userId,
        Domain.Entities.Notification notification,
        CancellationToken cancellationToken = default);

    Task SendToHub(
        Guid hubId,
        Domain.Entities.Notification notification,
        CancellationToken cancellationToken = default);
}
