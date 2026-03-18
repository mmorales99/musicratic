using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.DeleteNotification;

public sealed class DeleteNotificationHandler(
    INotificationRepository notificationRepository,
    INotificationUnitOfWork unitOfWork) : ICommandHandler<DeleteNotificationCommand>
{
    public async Task Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetById(
            request.NotificationId, cancellationToken);

        if (notification is null || notification.UserId != request.UserId)
            return;

        notificationRepository.Remove(notification);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
