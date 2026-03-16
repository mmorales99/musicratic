using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Musicratic.Playback.Application.Services;

namespace Musicratic.Playback.Infrastructure.Services;

public sealed class HubConnectionManager(
    ILogger<HubConnectionManager> logger) : IHubConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, WebSocket>> _connections = new();

    public void AddConnection(Guid hubId, string connectionId, WebSocket socket)
    {
        var hubConnections = _connections.GetOrAdd(hubId, _ => new ConcurrentDictionary<string, WebSocket>());
        hubConnections[connectionId] = socket;

        logger.LogInformation(
            "WebSocket connection {ConnectionId} added to hub {HubId}. Total: {Count}",
            connectionId, hubId, hubConnections.Count);
    }

    public void RemoveConnection(Guid hubId, string connectionId)
    {
        if (_connections.TryGetValue(hubId, out var hubConnections))
        {
            hubConnections.TryRemove(connectionId, out _);

            logger.LogInformation(
                "WebSocket connection {ConnectionId} removed from hub {HubId}. Remaining: {Count}",
                connectionId, hubId, hubConnections.Count);

            if (hubConnections.IsEmpty)
            {
                _connections.TryRemove(hubId, out _);
            }
        }
    }

    public IReadOnlyCollection<string> GetConnectionIds(Guid hubId)
    {
        if (_connections.TryGetValue(hubId, out var hubConnections))
        {
            return hubConnections.Keys.ToList().AsReadOnly();
        }
        return Array.Empty<string>();
    }

    public async Task SendToHub(
        Guid hubId, string message, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(hubId, out var hubConnections))
            return;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);
        var deadConnections = new List<string>();

        foreach (var (connectionId, socket) in hubConnections)
        {
            if (socket.State != WebSocketState.Open)
            {
                deadConnections.Add(connectionId);
                continue;
            }

            try
            {
                await socket.SendAsync(
                    segment, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (WebSocketException ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to send to connection {ConnectionId} in hub {HubId}",
                    connectionId, hubId);
                deadConnections.Add(connectionId);
            }
        }

        foreach (var dead in deadConnections)
        {
            RemoveConnection(hubId, dead);
        }
    }
}
