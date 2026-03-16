using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record TrackStartedPlayingEvent(
    Guid QueueEntryId,
    Guid TrackId,
    Guid HubId) : DomainEvent;
