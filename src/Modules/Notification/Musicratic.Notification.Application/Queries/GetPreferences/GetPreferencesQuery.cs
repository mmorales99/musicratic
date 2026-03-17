using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetPreferences;

public sealed record PreferenceDto(
    Guid Id,
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled);

public sealed record GetPreferencesQuery(Guid UserId) : IQuery<IReadOnlyList<PreferenceDto>>;
