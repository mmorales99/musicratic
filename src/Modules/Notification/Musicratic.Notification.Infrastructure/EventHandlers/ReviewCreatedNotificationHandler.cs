using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Infrastructure.EventHandlers;

/// <summary>
/// NTFY-009: Handles ReviewCreatedIntegrationEvent from Social module.
/// Dapr topic: {env}_social_review-created
/// Creates a notification about the review and pushes to the hub via WebSocket.
/// </summary>
public sealed class ReviewCreatedNotificationHandler
{
    /// <summary>Well-known ID for system-generated hub-level notifications.</summary>
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;
    private readonly INotificationPushService _pushService;
    private readonly ILogger<ReviewCreatedNotificationHandler> _logger;

    public ReviewCreatedNotificationHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork,
        INotificationPushService pushService,
        ILogger<ReviewCreatedNotificationHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(
        ReviewCreatedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var message = $"{@event.ReviewerName} reviewed \"{@event.TrackTitle}\" ({@event.Rating}/5)";

        var notification = Domain.Entities.Notification.Create(
            SystemUserId,
            NotificationType.ReviewCreated,
            "New Review",
            message,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                reviewId = @event.ReviewId,
                hubId = @event.HubId,
                reviewerId = @event.ReviewerId,
                trackId = @event.TrackId,
                rating = @event.Rating
            }));

        await _repository.Add(notification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        await _pushService.SendToHub(@event.HubId, notification, cancellationToken);

        _logger.LogInformation(
            "Processed review-created notification: hub {HubId}, reviewer {ReviewerId}, track {TrackId}",
            @event.HubId, @event.ReviewerId, @event.TrackId);
    }
}
