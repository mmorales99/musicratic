using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Api.Endpoints;

/// <summary>
/// VOTE-009: WebSocket endpoint for vote tally broadcasting.
/// Endpoint: /ws/hubs/{hubId}/votes
/// </summary>
public static class VoteWebSocketEndpoint
{
    public static IEndpointRouteBuilder MapVoteWebSocket(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/hubs/{hubId:guid}/votes", HandleWebSocket);
        return endpoints;
    }

    private static async Task HandleWebSocket(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("WebSocket connection expected.");
            return;
        }

        var hubIdStr = context.GetRouteValue("hubId")?.ToString();
        if (!Guid.TryParse(hubIdStr, out var hubId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid hub ID.");
            return;
        }

        var connectionManager = context.RequestServices
            .GetRequiredService<IVoteConnectionManager>();

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        connectionManager.AddConnection(hubId, connectionId, socket);

        try
        {
            await ReceiveLoop(socket, context.RequestAborted);
        }
        finally
        {
            connectionManager.RemoveConnection(hubId, connectionId);

            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed",
                    CancellationToken.None);
            }
        }
    }

    private static async Task ReceiveLoop(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
                break;
        }
    }
}
