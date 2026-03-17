namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Analytics module when a report has been generated.
/// Dapr topic: {env}_analytics_report-ready
/// Consumed by: Notification module.
/// </summary>
public sealed record ReportReadyIntegrationEvent(
    Guid ReportId,
    Guid HubId,
    Guid RequestedByUserId,
    string ReportType,
    DateTime GeneratedAt);
