using MediatR;
using Microsoft.Extensions.Logging;
using Musicratic.Analytics.Application.Services;

namespace Musicratic.Analytics.Infrastructure.EventHandlers;

/// <summary>
/// ANLT-010: Handles skip-triggered integration events from Voting module.
/// Dapr topic: {env}_voting_skip-triggered → increments skip count.
/// TenantId serves as HubId (hub = tenant in Musicratic).
/// </summary>
public sealed record SkipTriggeredAnalyticsNotification(
    Guid TenantId,
    Guid QueueEntryId,
    Guid TrackId,
    string Reason,
    double DownvotePercentage) : INotification;

public sealed class SkipTriggeredAnalyticsHandler(
    IStatsAggregationService statsAggregationService,
    ILogger<SkipTriggeredAnalyticsHandler> logger)
    : INotificationHandler<SkipTriggeredAnalyticsNotification>
{
    public async Task Handle(
        SkipTriggeredAnalyticsNotification notification,
        CancellationToken cancellationToken)
    {
        if (notification.TrackId == Guid.Empty)
        {
            logger.LogWarning(
                "Skip event for entry {EntryId} has no TrackId. Skipping track stats.",
                notification.QueueEntryId);
            return;
        }

        await statsAggregationService.RecordSkip(
            notification.TrackId,
            notification.TenantId,
            notification.TenantId,
            cancellationToken);

        logger.LogInformation(
            "Processed skip event: track {TrackId}, hub {HubId}, reason={Reason}",
            notification.TrackId, notification.TenantId, notification.Reason);
    }
}
