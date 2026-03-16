namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Playback module when a track ends.
/// Dapr topic: {env}_playback_track-ended
/// Consumed by: Voting module (to close voting window).
/// </summary>
public sealed record TrackEndedIntegrationEvent(
    Guid HubId,
    Guid QueueEntryId,
    Guid TrackId);
