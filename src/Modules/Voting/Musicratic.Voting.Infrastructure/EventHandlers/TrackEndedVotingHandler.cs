using MediatR;
using Microsoft.Extensions.Logging;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Infrastructure.EventHandlers;

/// <summary>
/// VOTE-013: Handles track-ended event from Playback module.
/// Dapr topic: {env}_playback_track-ended → closes voting window.
/// Currently wired via MediatR notification; will be replaced by Dapr subscription.
/// </summary>
public sealed record TrackEndedNotification(
    Guid HubId,
    Guid QueueEntryId) : INotification;

public sealed class TrackEndedVotingHandler(
    IVotingWindowService votingWindowService,
    ILogger<TrackEndedVotingHandler> logger)
    : INotificationHandler<TrackEndedNotification>
{
    public Task Handle(
        TrackEndedNotification notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Track ended in hub {HubId}, entry {EntryId}. Closing voting window.",
            notification.HubId, notification.QueueEntryId);

        votingWindowService.CloseWindow(notification.HubId, notification.QueueEntryId);

        return Task.CompletedTask;
    }
}
