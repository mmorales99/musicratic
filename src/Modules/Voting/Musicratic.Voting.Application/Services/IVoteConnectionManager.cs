using System.Net.WebSockets;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// Manages WebSocket connections for vote tally broadcasting.
/// Endpoint: /ws/hubs/{hubId}/votes
/// </summary>
public interface IVoteConnectionManager
{
    void AddConnection(Guid hubId, string connectionId, WebSocket socket);

    void RemoveConnection(Guid hubId, string connectionId);

    int GetConnectionCount(Guid hubId);

    Task SendToHub(Guid hubId, string message, CancellationToken cancellationToken = default);
}
