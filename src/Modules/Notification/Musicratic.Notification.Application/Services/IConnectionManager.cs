namespace Musicratic.Notification.Application.Services;

public interface IConnectionManager
{
    void AddConnection(Guid userId, string connectionId);

    void RemoveConnection(Guid userId, string connectionId);

    IReadOnlySet<string> GetConnections(Guid userId);
}
