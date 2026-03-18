using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadHandler(
    INotificationRepository notificationRepository,
    INotificationUnitOfWork unitOfWork) : ICommandHandler<MarkNotificationReadCommand>
{
    public async Task Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetById(
            request.NotificationId, cancellationToken);

        if (notification is null || notification.UserId != request.UserId)
            return;

        notification.MarkAsRead();
        notificationRepository.Update(notification);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
