using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Notification.Infrastructure.EventHandlers;
using Musicratic.Shared.Contracts.Events;

namespace Musicratic.Notification.Api.Endpoints;

/// <summary>
/// NTFY-009: Dapr subscriber endpoints for notification event handling.
/// Each endpoint receives an integration event from Dapr pub/sub and
/// delegates to the corresponding handler service class.
/// </summary>
public static class NotificationDaprSubscriberEndpoints
{
    public static RouteGroupBuilder MapNotificationDaprEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications/dapr")
            .WithTags("Notification Dapr");

        group.MapPost("/vote-cast", HandleVoteCast)
            .WithName("NotificationVoteCast");

        group.MapPost("/skip-triggered", HandleSkipTriggered)
            .WithName("NotificationSkipTriggered");

        group.MapPost("/track-started", HandleTrackStarted)
            .WithName("NotificationTrackStarted");

        group.MapPost("/review-created", HandleReviewCreated)
            .WithName("NotificationReviewCreated");

        group.MapPost("/report-ready", HandleReportReady)
            .WithName("NotificationReportReady");

        return group;
    }

    private static async Task<IResult> HandleVoteCast(
        VoteCastIntegrationEvent @event,
        VoteCastNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(@event, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> HandleSkipTriggered(
        SkipTriggeredIntegrationEvent @event,
        SkipTriggeredNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(@event, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> HandleTrackStarted(
        TrackStartedIntegrationEvent @event,
        TrackStartedNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(@event, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> HandleReviewCreated(
        ReviewCreatedIntegrationEvent @event,
        ReviewCreatedNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(@event, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> HandleReportReady(
        ReportReadyIntegrationEvent @event,
        ReportReadyNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(@event, cancellationToken);
        return Results.Ok();
    }
}
