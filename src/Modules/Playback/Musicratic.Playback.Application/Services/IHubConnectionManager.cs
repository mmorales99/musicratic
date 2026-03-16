using System.Net.WebSockets;

namespace Musicratic.Playback.Application.Services;

public interface IHubConnectionManager
{
    void AddConnection(Guid hubId, string connectionId, WebSocket socket);

    void RemoveConnection(Guid hubId, string connectionId);

    IReadOnlyCollection<string> GetConnectionIds(Guid hubId);

    Task SendToHub(Guid hubId, string message, CancellationToken cancellationToken = default);
}
