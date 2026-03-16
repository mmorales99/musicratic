namespace Musicratic.Notification.Application.Models;

public sealed record PushResult(
    bool Success,
    string? ErrorReason = null,
    bool ShouldRemoveToken = false);
