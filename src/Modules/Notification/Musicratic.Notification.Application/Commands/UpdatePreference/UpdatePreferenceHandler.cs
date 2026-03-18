using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.UpdatePreference;

public sealed class UpdatePreferenceHandler(
    INotificationPreferenceRepository preferenceRepository,
    INotificationUnitOfWork unitOfWork) : ICommandHandler<UpdatePreferenceCommand>
{
    public async Task Handle(
        UpdatePreferenceCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await preferenceRepository.GetByUserId(
            request.UserId, cancellationToken);

        foreach (var item in request.Preferences)
        {
            var match = existing.FirstOrDefault(p =>
                p.NotificationType == item.NotificationType &&
                p.Channel == item.Channel);

            if (match is not null)
            {
                if (item.IsEnabled)
                    match.Enable();
                else
                    match.Disable();

                preferenceRepository.Update(match);
            }
            else
            {
                var preference = NotificationPreference.Create(
                    request.UserId,
                    request.TenantId,
                    item.NotificationType,
                    item.Channel,
                    item.IsEnabled);

                await preferenceRepository.Upsert(preference, cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
