using System.Collections.Concurrent;
using Musicratic.Notification.Application.Services;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class InMemoryConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _connections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        var userConnections = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        userConnections.TryAdd(connectionId, 0);
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
            return;

        userConnections.TryRemove(connectionId, out _);

        if (userConnections.IsEmpty)
            _connections.TryRemove(userId, out _);
    }

    public IReadOnlySet<string> GetConnections(Guid userId)
    {
        if (_connections.TryGetValue(userId, out var userConnections))
            return userConnections.Keys.ToHashSet();

        return new HashSet<string>();
    }
}
