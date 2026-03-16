using Microsoft.Extensions.Logging;
using Musicratic.Notification.Application.Models;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Enums;
using Musicratic.Notification.Domain.Repositories;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class PushNotificationService : IPushNotificationService
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly IApnsPushService _apnsPushService;
    private readonly IFcmPushService _fcmPushService;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IDeviceTokenRepository deviceTokenRepository,
        IApnsPushService apnsPushService,
        IFcmPushService fcmPushService,
        ILogger<PushNotificationService> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _apnsPushService = apnsPushService;
        _fcmPushService = fcmPushService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PushResult>> SendToUser(
        Guid userId,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var devices = await _deviceTokenRepository.GetByUserId(userId, cancellationToken);

        if (devices.Count == 0)
        {
            _logger.LogDebug("No registered devices for user {UserId}", userId);
            return [];
        }

        var results = new List<PushResult>(devices.Count);

        foreach (var device in devices)
        {
            var result = device.Platform switch
            {
                DevicePlatform.iOS => await _apnsPushService.SendPush(
                    device.Token, title, body, data, cancellationToken),
                DevicePlatform.Android => await _fcmPushService.SendPush(
                    device.Token, title, body, data, cancellationToken),
                _ => new PushResult(false, $"Unknown platform: {device.Platform}")
            };

            if (result.ShouldRemoveToken)
            {
                _logger.LogInformation(
                    "Deactivating invalid device token {TokenId} for user {UserId}",
                    device.Id,
                    userId);
                device.Deactivate();
                _deviceTokenRepository.Update(device);
            }

            results.Add(result);
        }

        return results;
    }
}
