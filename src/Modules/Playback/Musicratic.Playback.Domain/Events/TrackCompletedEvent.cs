using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record TrackCompletedEvent(
    Guid QueueEntryId,
    Guid TrackId,
    Guid HubId) : DomainEvent;
