using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public Guid UserId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? DataJson { get; private set; }

    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? dataJson = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            DataJson = dataJson
        };
    }

    public void MarkAsRead()
    {
        ReadAt ??= DateTime.UtcNow;
    }
}
