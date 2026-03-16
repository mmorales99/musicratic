using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Domain.Events;

public sealed record CollectiveVoteClosedEvent(
    Guid SessionId,
    Guid TenantId,
    Guid QueueEntryId,
    Guid ProposerId,
    CollectiveVoteStatus Status) : DomainEvent;
