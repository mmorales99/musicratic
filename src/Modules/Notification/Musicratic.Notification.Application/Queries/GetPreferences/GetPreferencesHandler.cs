using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetPreferences;

public sealed class GetPreferencesHandler(
    INotificationPreferenceRepository preferenceRepository) : IQueryHandler<GetPreferencesQuery, IReadOnlyList<PreferenceDto>>
{
    public async Task<IReadOnlyList<PreferenceDto>> Handle(
        GetPreferencesQuery request,
        CancellationToken cancellationToken)
    {
        var preferences = await preferenceRepository.GetByUserId(
            request.UserId, cancellationToken);

        // Fill defaults for any missing type/channel combinations
        var result = new List<PreferenceDto>();
        var notificationTypes = Enum.GetValues<NotificationType>();
        var channels = Enum.GetValues<NotificationChannel>();

        foreach (var type in notificationTypes)
        {
            foreach (var channel in channels)
            {
                var existing = preferences.FirstOrDefault(p =>
                    p.NotificationType == type && p.Channel == channel);

                result.Add(new PreferenceDto(
                    Id: existing?.Id ?? Guid.Empty,
                    NotificationType: type,
                    Channel: channel,
                    IsEnabled: existing?.IsEnabled ?? true));
            }
        }

        return result;
    }
}
