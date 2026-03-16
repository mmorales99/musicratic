using Musicratic.Notification.Application.Models;

namespace Musicratic.Notification.Application.Services;

public interface IFcmPushService
{
    Task<PushResult> SendPush(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
