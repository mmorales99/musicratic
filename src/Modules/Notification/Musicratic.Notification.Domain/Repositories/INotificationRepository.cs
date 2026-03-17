using Musicratic.Notification.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Repositories;

public interface INotificationRepository : IRepository<Entities.Notification>
{
    Task<IReadOnlyList<Entities.Notification>> GetByUser(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Entities.Notification> Items, int TotalCount)> GetByUserPaginated(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCount(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Entities.Notification>> GetUnreadByUser(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}
