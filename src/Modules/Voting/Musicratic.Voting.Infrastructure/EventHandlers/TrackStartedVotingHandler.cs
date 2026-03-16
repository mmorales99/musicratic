using MediatR;
using Microsoft.Extensions.Logging;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Infrastructure.EventHandlers;

/// <summary>
/// VOTE-013: Handles track-started event from Playback module.
/// Dapr topic: {env}_playback_track-started → opens voting window for new track.
/// Currently wired via MediatR notification; will be replaced by Dapr subscription.
/// </summary>
public sealed record TrackStartedNotification(
    Guid HubId,
    Guid QueueEntryId) : INotification;

public sealed class TrackStartedVotingHandler(
    IVotingWindowService votingWindowService,
    ILogger<TrackStartedVotingHandler> logger)
    : INotificationHandler<TrackStartedNotification>
{
    public Task Handle(
        TrackStartedNotification notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Track started in hub {HubId}, entry {EntryId}. Opening voting window.",
            notification.HubId, notification.QueueEntryId);

        votingWindowService.OpenWindow(notification.HubId, notification.QueueEntryId);

        return Task.CompletedTask;
    }
}
