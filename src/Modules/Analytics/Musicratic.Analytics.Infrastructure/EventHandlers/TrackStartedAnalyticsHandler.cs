using MediatR;
using Microsoft.Extensions.Logging;
using Musicratic.Analytics.Application.Services;

namespace Musicratic.Analytics.Infrastructure.EventHandlers;

/// <summary>
/// ANLT-010: Handles track-started integration events from Playback module.
/// Dapr topic: {env}_playback_track-started → increments play count + hub activity.
/// </summary>
public sealed record TrackStartedAnalyticsNotification(
    Guid HubId,
    Guid QueueEntryId,
    Guid TrackId) : INotification;

public sealed class TrackStartedAnalyticsHandler(
    IStatsAggregationService statsAggregationService,
    ILogger<TrackStartedAnalyticsHandler> logger)
    : INotificationHandler<TrackStartedAnalyticsNotification>
{
    public async Task Handle(
        TrackStartedAnalyticsNotification notification,
        CancellationToken cancellationToken)
    {
        // Record play with zero duration; actual duration updated on track-ended
        await statsAggregationService.RecordTrackPlay(
            notification.TrackId,
            notification.HubId,
            notification.HubId,
            TimeSpan.Zero,
            cancellationToken);

        // Record hub activity (1 concurrent user minimum per play)
        await statsAggregationService.RecordHubActivity(
            notification.HubId,
            notification.HubId,
            1,
            cancellationToken);

        logger.LogInformation(
            "Processed track-started event: track {TrackId} in hub {HubId}",
            notification.TrackId, notification.HubId);
    }
}
