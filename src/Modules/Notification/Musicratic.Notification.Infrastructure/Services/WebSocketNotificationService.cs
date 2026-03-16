using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Services;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class WebSocketNotificationService : INotificationPushService
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<WebSocketNotificationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WebSocketNotificationService(
        IConnectionManager connectionManager,
        ILogger<WebSocketNotificationService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public void RegisterSocket(string connectionId, WebSocket webSocket)
    {
        _sockets.TryAdd(connectionId, webSocket);
    }

    public void UnregisterSocket(string connectionId)
    {
        _sockets.TryRemove(connectionId, out _);
    }

    public async Task SendToUser(
        Guid userId,
        Domain.Entities.Notification notification,
        CancellationToken cancellationToken = default)
    {
        var connectionIds = _connectionManager.GetConnections(userId);
        var envelope = CreateEnvelope(notification);
        var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);

        foreach (var connectionId in connectionIds)
        {
            await SendToSocket(connectionId, payload, cancellationToken);
        }
    }

    public async Task SendToHub(
        Guid hubId,
        Domain.Entities.Notification notification,
        CancellationToken cancellationToken = default)
    {
        // Hub broadcast requires member-connection mapping; for now log and skip.
        // Full implementation requires hub membership resolution from Hub module via Dapr.
        _logger.LogWarning(
            "SendToHub not fully implemented yet. HubId: {HubId}, NotificationId: {NotificationId}",
            hubId,
            notification.Id);

        await Task.CompletedTask;
    }

    private async Task SendToSocket(
        string connectionId,
        byte[] payload,
        CancellationToken cancellationToken)
    {
        if (!_sockets.TryGetValue(connectionId, out var socket))
            return;

        if (socket.State != WebSocketState.Open)
        {
            _sockets.TryRemove(connectionId, out _);
            return;
        }

        try
        {
            await socket.SendAsync(
                new ArraySegment<byte>(payload),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "Failed to send WebSocket message to {ConnectionId}", connectionId);
            _sockets.TryRemove(connectionId, out _);
        }
    }

    private static object CreateEnvelope(Domain.Entities.Notification notification)
    {
        return new
        {
            Type = "notification",
            Payload = new
            {
                notification.Id,
                notification.UserId,
                Type = notification.Type.ToString(),
                notification.Title,
                notification.Body,
                notification.DataJson,
                notification.CreatedAt
            }
        };
    }
}
