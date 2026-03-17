using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.MarkAllRead;

public sealed class MarkAllReadHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<MarkAllReadCommand>
{
    public async Task Handle(
        MarkAllReadCommand request,
        CancellationToken cancellationToken)
    {
        var unread = await notificationRepository.GetUnreadByUser(
            request.UserId, cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            notificationRepository.Update(notification);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
