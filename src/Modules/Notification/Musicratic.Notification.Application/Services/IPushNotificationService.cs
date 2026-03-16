using Musicratic.Notification.Application.Models;

namespace Musicratic.Notification.Application.Services;

public interface IPushNotificationService
{
    Task<IReadOnlyList<PushResult>> SendToUser(
        Guid userId,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
