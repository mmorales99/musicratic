using MediatR;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.EventHandlers;

/// <summary>
/// MediatR notification for hub deactivation. Will be replaced by Dapr
/// subscription when cross-module pub/sub is wired up.
/// Topic: {env}_hub_deactivated
/// </summary>
public sealed record HubDeactivatedNotification(Guid HubId) : INotification;

public sealed class HubDeactivatedEventHandler(
    IQueueEntryRepository queueEntryRepository,
    IPlaybackUnitOfWork unitOfWork) : INotificationHandler<HubDeactivatedNotification>
{
    public async Task Handle(
        HubDeactivatedNotification notification, CancellationToken cancellationToken)
    {
        var entries = await queueEntryRepository.GetByHubId(
            notification.HubId, cancellationToken);

        foreach (var entry in entries)
        {
            if (entry.Status is QueueEntryStatus.Playing)
            {
                entry.Skip();
                queueEntryRepository.Update(entry);
            }
            else if (entry.Status is QueueEntryStatus.Queued or QueueEntryStatus.Pending)
            {
                queueEntryRepository.Remove(entry);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
