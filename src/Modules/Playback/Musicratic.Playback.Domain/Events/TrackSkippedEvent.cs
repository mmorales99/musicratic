using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record TrackSkippedEvent(
    Guid QueueEntryId,
    Guid TrackId,
    Guid HubId,
    bool WasPlaying) : DomainEvent;
