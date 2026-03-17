using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.UpdatePreference;

public sealed record UpdatePreferenceItem(
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled);

public sealed record UpdatePreferenceCommand(
    Guid UserId,
    Guid TenantId,
    IReadOnlyList<UpdatePreferenceItem> Preferences) : ICommand;
