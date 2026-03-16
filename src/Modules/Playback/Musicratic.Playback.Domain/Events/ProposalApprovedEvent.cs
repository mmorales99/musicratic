using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record ProposalApprovedEvent(
    Guid QueueEntryId,
    Guid TrackId,
    Guid HubId) : DomainEvent;
