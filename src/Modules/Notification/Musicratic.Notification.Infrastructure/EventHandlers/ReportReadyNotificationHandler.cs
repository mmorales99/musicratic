using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Infrastructure.EventHandlers;

/// <summary>
/// NTFY-009: Handles ReportReadyIntegrationEvent from Analytics module.
/// Dapr topic: {env}_analytics_report-ready
/// Creates a notification for the requesting user and pushes via WebSocket.
/// </summary>
public sealed class ReportReadyNotificationHandler
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPushService _pushService;
    private readonly ILogger<ReportReadyNotificationHandler> _logger;

    public ReportReadyNotificationHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        INotificationPushService pushService,
        ILogger<ReportReadyNotificationHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(
        ReportReadyIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var message = $"Your {FormatReportType(@event.ReportType)} report is ready";

        var notification = Domain.Entities.Notification.Create(
            @event.RequestedByUserId,
            NotificationType.ReportReady,
            "Report Ready",
            message,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                reportId = @event.ReportId,
                hubId = @event.HubId,
                reportType = @event.ReportType,
                generatedAt = @event.GeneratedAt
            }));

        await _repository.Add(notification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        await _pushService.SendToUser(
            @event.RequestedByUserId, notification, cancellationToken);

        _logger.LogInformation(
            "Processed report-ready notification: user {UserId}, report {ReportId}, type={ReportType}",
            @event.RequestedByUserId, @event.ReportId, @event.ReportType);
    }

    private static string FormatReportType(string reportType)
    {
        return reportType.Replace('_', ' ').Replace('-', ' ');
    }
}
