using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Repositories;

public interface INotificationPreferenceRepository : IRepository<NotificationPreference>
{
    Task<IReadOnlyList<NotificationPreference>> GetByUserId(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationPreference>> GetByUserIdAndType(
        Guid userId,
        NotificationType notificationType,
        CancellationToken cancellationToken = default);

    Task Upsert(
        NotificationPreference preference,
        CancellationToken cancellationToken = default);
}
