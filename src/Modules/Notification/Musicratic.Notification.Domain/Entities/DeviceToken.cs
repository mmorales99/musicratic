using Musicratic.Notification.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Notification.Domain.Entities;

public sealed class DeviceToken : BaseEntity
{
    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DevicePlatform Platform { get; private set; }

    public string? DeviceName { get; private set; }

    public bool IsActive { get; private set; }

    private DeviceToken() { }

    public static DeviceToken Create(
        Guid userId,
        string token,
        DevicePlatform platform,
        string? deviceName = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return new DeviceToken
        {
            UserId = userId,
            Token = token,
            Platform = platform,
            DeviceName = deviceName,
            IsActive = true
        };
    }

    public void Update(string? deviceName)
    {
        DeviceName = deviceName;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
