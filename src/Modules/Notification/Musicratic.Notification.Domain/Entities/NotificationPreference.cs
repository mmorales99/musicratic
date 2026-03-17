using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Entities;

public sealed class NotificationPreference : BaseEntity, ITenantScoped
{
    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public NotificationType NotificationType { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public bool IsEnabled { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference Create(
        Guid userId,
        Guid tenantId,
        NotificationType notificationType,
        NotificationChannel channel,
        bool isEnabled = true)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new NotificationPreference
        {
            UserId = userId,
            TenantId = tenantId,
            NotificationType = notificationType,
            Channel = channel,
            IsEnabled = isEnabled
        };
    }

    public void Toggle()
    {
        IsEnabled = !IsEnabled;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
