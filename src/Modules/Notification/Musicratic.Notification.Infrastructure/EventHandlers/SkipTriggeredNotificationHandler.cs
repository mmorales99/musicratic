using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Infrastructure.EventHandlers;

/// <summary>
/// NTFY-009: Handles SkipTriggeredIntegrationEvent from Voting module.
/// Dapr topic: {env}_voting_skip-triggered
/// Creates a hub-level notification when a track is skipped and pushes via WebSocket.
/// </summary>
public sealed class SkipTriggeredNotificationHandler
{
    /// <summary>Well-known ID for system-generated hub-level notifications.</summary>
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPushService _pushService;
    private readonly ILogger<SkipTriggeredNotificationHandler> _logger;

    public SkipTriggeredNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        INotificationPushService pushService,
        ILogger<SkipTriggeredNotificationHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(
        SkipTriggeredIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var hubId = @event.TenantId;
        var message = $"Track was skipped — {FormatReason(@event.Reason, @event.DownvotePercentage)}";

        var notification = Domain.Entities.Notification.Create(
            SystemUserId,
            NotificationType.TrackSkipped,
            "Track Skipped",
            message,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                hubId,
                queueEntryId = @event.QueueEntryId,
                reason = @event.Reason,
                downvotePercentage = @event.DownvotePercentage
            }));

        await _repository.Add(notification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        await _pushService.SendToHub(hubId, notification, cancellationToken);

        _logger.LogInformation(
            "Processed skip-triggered notification: hub {HubId}, reason={Reason}",
            hubId, @event.Reason);
    }

    private static string FormatReason(string reason, double downvotePercentage)
    {
        return downvotePercentage > 0
            ? $"{reason} ({downvotePercentage:F0}% downvotes)"
            : reason;
    }
}
