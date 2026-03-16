using MediatR;
using Musicratic.Playback.Application.Services;

namespace Musicratic.Playback.Application.EventHandlers;

/// <summary>
/// MediatR notification for hub activation. Will be replaced by Dapr
/// subscription when cross-module pub/sub is wired up.
/// Topic: {env}_hub_activated
/// </summary>
public sealed record HubActivatedNotification(Guid HubId) : INotification;

public sealed class HubActivatedEventHandler(
    IPlaybackOrchestrator orchestrator) : INotificationHandler<HubActivatedNotification>
{
    public async Task Handle(
        HubActivatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await orchestrator.StartPlayback(notification.HubId, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // No queued tracks or already playing — not a fatal error.
            // Logging happens inside PlaybackOrchestrator.
        }
    }
}
