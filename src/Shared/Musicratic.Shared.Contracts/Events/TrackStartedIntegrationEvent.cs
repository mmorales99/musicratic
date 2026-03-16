namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Playback module when a track starts playing.
/// Dapr topic: {env}_playback_track-started
/// Consumed by: Voting module (to open voting window).
/// </summary>
public sealed record TrackStartedIntegrationEvent(
    Guid HubId,
    Guid QueueEntryId,
    Guid TrackId);
