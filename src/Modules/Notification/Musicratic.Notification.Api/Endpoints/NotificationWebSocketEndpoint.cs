using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Infrastructure.Services;

namespace Musicratic.Notification.Api.Endpoints;

public static class NotificationWebSocketEndpoint
{
    public static IEndpointRouteBuilder MapNotificationWebSocket(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/notifications", HandleWebSocket);
        return endpoints;
    }

    private static async Task HandleWebSocket(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var userId = ExtractUserId(context);
        if (userId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var connectionManager = context.RequestServices.GetRequiredService<IConnectionManager>();
        var pushService = context.RequestServices.GetRequiredService<INotificationPushService>();
        var logger = context.RequestServices.GetRequiredService<ILogger<WebSocketNotificationService>>();

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        connectionManager.AddConnection(userId.Value, connectionId);

        if (pushService is WebSocketNotificationService wsService)
        {
            wsService.RegisterSocket(connectionId, webSocket);
        }

        logger.LogInformation(
            "WebSocket connected. UserId: {UserId}, ConnectionId: {ConnectionId}",
            userId.Value,
            connectionId);

        try
        {
            await KeepAlive(webSocket, context.RequestAborted);
        }
        finally
        {
            connectionManager.RemoveConnection(userId.Value, connectionId);

            if (pushService is WebSocketNotificationService wsServiceCleanup)
            {
                wsServiceCleanup.UnregisterSocket(connectionId);
            }

            logger.LogInformation(
                "WebSocket disconnected. UserId: {UserId}, ConnectionId: {ConnectionId}",
                userId.Value,
                connectionId);
        }
    }

    private static Guid? ExtractUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        if (Guid.TryParse(sub, out var userId))
            return userId;

        return null;
    }

    private static async Task KeepAlive(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing",
                    cancellationToken);
                break;
            }
        }
    }
}
