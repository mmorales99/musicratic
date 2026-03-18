using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Infrastructure.EventHandlers;

/// <summary>
/// NTFY-009: Handles TrackStartedIntegrationEvent from Playback module.
/// Dapr topic: {env}_playback_track-started
/// Creates a "Now playing" hub-level notification and pushes via WebSocket.
/// </summary>
public sealed class TrackStartedNotificationHandler
{
    /// <summary>Well-known ID for system-generated hub-level notifications.</summary>
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly INotificationPushService _pushService;
    private readonly ILogger<TrackStartedNotificationHandler> _logger;

    public TrackStartedNotificationHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        INotificationPushService pushService,
        ILogger<TrackStartedNotificationHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(
        TrackStartedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        // TrackStartedIntegrationEvent does not include track title;
        // a future enrichment via Dapr service invocation to Playback module could resolve it.
        var message = "Now playing: new track started";

        var notification = Domain.Entities.Notification.Create(
            SystemUserId,
            NotificationType.NowPlaying,
            "Now Playing",
            message,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                hubId = @event.HubId,
                queueEntryId = @event.QueueEntryId,
                trackId = @event.TrackId
            }));

        await _repository.Add(notification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        await _pushService.SendToHub(@event.HubId, notification, cancellationToken);

        _logger.LogInformation(
            "Processed track-started notification: hub {HubId}, track {TrackId}",
            @event.HubId, @event.TrackId);
    }
}
