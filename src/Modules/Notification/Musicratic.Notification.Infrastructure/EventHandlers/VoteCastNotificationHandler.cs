using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Infrastructure.EventHandlers;

/// <summary>
/// NTFY-009: Handles VoteCastIntegrationEvent from Voting module.
/// Dapr topic: {env}_voting_vote-cast
/// Creates a notification for the voter and pushes to the hub via WebSocket.
/// </summary>
public sealed class VoteCastNotificationHandler
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPushService _pushService;
    private readonly ILogger<VoteCastNotificationHandler> _logger;

    public VoteCastNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        INotificationPushService pushService,
        ILogger<VoteCastNotificationHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(
        VoteCastIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var hubId = @event.TenantId; // TenantId = HubId in Musicratic
        var message = $"Vote '{@event.Value}' cast on queue entry";

        var notification = Domain.Entities.Notification.Create(
            @event.UserId,
            NotificationType.VoteCast,
            "Vote Cast",
            message,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                hubId,
                queueEntryId = @event.QueueEntryId,
                value = @event.Value
            }));

        await _repository.Add(notification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        await _pushService.SendToHub(hubId, notification, cancellationToken);

        _logger.LogInformation(
            "Processed vote-cast notification: hub {HubId}, user {UserId}, vote={Value}",
            hubId, @event.UserId, @event.Value);
    }
}
