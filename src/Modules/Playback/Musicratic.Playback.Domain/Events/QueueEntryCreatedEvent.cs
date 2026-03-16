using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record QueueEntryCreatedEvent(
    Guid QueueEntryId,
    Guid TrackId,
    Guid HubId) : DomainEvent;
