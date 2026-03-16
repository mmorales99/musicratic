using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Infrastructure.Services;

/// <summary>
/// VOTE-009: Manages WebSocket connections for vote tally broadcasting.
/// Mirrors Playback.Infrastructure.Services.HubConnectionManager pattern.
/// </summary>
public sealed class VoteConnectionManager(
    ILogger<VoteConnectionManager> logger) : IVoteConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, WebSocket>> _connections = new();

    public void AddConnection(Guid hubId, string connectionId, WebSocket socket)
    {
        var hubConnections = _connections.GetOrAdd(
            hubId, _ => new ConcurrentDictionary<string, WebSocket>());
        hubConnections[connectionId] = socket;

        logger.LogInformation(
            "Vote WebSocket {ConnectionId} added to hub {HubId}. Total: {Count}",
            connectionId, hubId, hubConnections.Count);
    }

    public void RemoveConnection(Guid hubId, string connectionId)
    {
        if (!_connections.TryGetValue(hubId, out var hubConnections))
            return;

        hubConnections.TryRemove(connectionId, out _);

        logger.LogInformation(
            "Vote WebSocket {ConnectionId} removed from hub {HubId}. Remaining: {Count}",
            connectionId, hubId, hubConnections.Count);

        if (hubConnections.IsEmpty)
        {
            _connections.TryRemove(hubId, out _);
        }
    }

    public int GetConnectionCount(Guid hubId)
    {
        return _connections.TryGetValue(hubId, out var c) ? c.Count : 0;
    }

    public async Task SendToHub(
        Guid hubId, string message, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(hubId, out var hubConnections))
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);
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
                logger.LogWarning(ex,
                    "Failed to send to vote connection {ConnectionId} in hub {HubId}",
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
