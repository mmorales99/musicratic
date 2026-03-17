using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Notification.Application.Commands.DeleteNotification;
using Musicratic.Notification.Application.Commands.MarkAllRead;
using Musicratic.Notification.Application.Commands.MarkNotificationRead;
using Musicratic.Notification.Application.Commands.UpdatePreference;
using Musicratic.Notification.Application.Queries.GetNotifications;
using Musicratic.Notification.Application.Queries.GetPreferences;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;

namespace Musicratic.Notification.Api.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications").WithTags("Notifications");

        group.MapGet("/", GetNotifications).WithName("GetNotifications");
        group.MapGet("/unread-count", GetUnreadCount).WithName("GetUnreadCount");
        group.MapPut("/{id:guid}/read", MarkAsRead).WithName("MarkNotificationRead");
        group.MapPut("/read-all", MarkAllRead).WithName("MarkAllRead");
        group.MapDelete("/{id:guid}", DeleteNotification).WithName("DeleteNotification");
        group.MapGet("/preferences", GetPreferences).WithName("GetPreferences");
        group.MapPut("/preferences", UpdatePreferences).WithName("UpdatePreferences");

        return group;
    }

    private static async Task<IResult> GetNotifications(
        int page,
        int pageSize,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await sender.Send(
            new GetNotificationsQuery(userId.Value, page, pageSize),
            cancellationToken);

        return Results.Ok(new
        {
            success = result.Success,
            total_items_in_response = result.TotalItemsInResponse,
            has_more_items = result.HasMoreItems,
            items = result.Items,
            audit = new { timestamp = result.Audit.Timestamp }
        });
    }

    private static async Task<IResult> GetUnreadCount(
        INotificationRepository notificationRepository,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        var count = await notificationRepository.GetUnreadCount(
            userId.Value, cancellationToken);

        return Results.Ok(new { success = true, unread_count = count });
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        await sender.Send(
            new MarkNotificationReadCommand(id, userId.Value),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> MarkAllRead(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        await sender.Send(
            new MarkAllReadCommand(userId.Value),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteNotification(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        await sender.Send(
            new DeleteNotificationCommand(id, userId.Value),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetPreferences(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        var preferences = await sender.Send(
            new GetPreferencesQuery(userId.Value),
            cancellationToken);

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = preferences.Count,
            has_more_items = false,
            items = preferences.Select(p => new
            {
                id = p.Id,
                notification_type = p.NotificationType.ToString(),
                channel = p.Channel.ToString(),
                is_enabled = p.IsEnabled
            })
        });
    }

    private static async Task<IResult> UpdatePreferences(
        UpdatePreferencesRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        var tenantId = ExtractTenantId(httpContext);

        var items = request.Preferences.Select(p =>
            new UpdatePreferenceItem(p.NotificationType, p.Channel, p.IsEnabled))
            .ToList();

        await sender.Send(
            new UpdatePreferenceCommand(userId.Value, tenantId, items),
            cancellationToken);

        return Results.NoContent();
    }

    private static Guid? ExtractUserId(HttpContext context)
    {
        var sub = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(sub, out var userId))
            return userId;

        return null;
    }

    private static Guid ExtractTenantId(HttpContext context)
    {
        var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public sealed record UpdatePreferencesRequest(
    IReadOnlyList<PreferenceItemRequest> Preferences);

public sealed record PreferenceItemRequest(
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled);
