using MediatR;
using Microsoft.Extensions.Logging;
using Musicratic.Analytics.Application.Services;

namespace Musicratic.Analytics.Infrastructure.EventHandlers;

/// <summary>
/// ANLT-010: Handles vote-cast integration events from Voting module.
/// Dapr topic: {env}_voting_vote-cast → increments track upvote/downvote counts.
/// TenantId serves as HubId (hub = tenant in Musicratic).
/// </summary>
public sealed record VoteCastAnalyticsNotification(
    Guid TenantId,
    Guid QueueEntryId,
    Guid TrackId,
    Guid UserId,
    string Value) : INotification;

public sealed class VoteCastAnalyticsHandler(
    IStatsAggregationService statsAggregationService,
    ILogger<VoteCastAnalyticsHandler> logger)
    : INotificationHandler<VoteCastAnalyticsNotification>
{
    public async Task Handle(
        VoteCastAnalyticsNotification notification,
        CancellationToken cancellationToken)
    {
        if (notification.TrackId == Guid.Empty)
        {
            logger.LogWarning(
                "Vote cast event for entry {EntryId} has no TrackId. Skipping track stats.",
                notification.QueueEntryId);
            return;
        }

        var isUpvote = notification.Value.Equals("Up", StringComparison.OrdinalIgnoreCase);

        await statsAggregationService.RecordVote(
            notification.TrackId,
            notification.TenantId,
            notification.TenantId,
            isUpvote,
            cancellationToken);

        logger.LogInformation(
            "Processed vote-cast event: track {TrackId}, hub {HubId}, vote={Value}",
            notification.TrackId, notification.TenantId, notification.Value);
    }
}
