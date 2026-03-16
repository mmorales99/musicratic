using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Domain.Events;

public sealed record VoteCastEvent(
    Guid VoteId,
    Guid TenantId,
    Guid QueueEntryId,
    Guid UserId,
    VoteValue Value) : DomainEvent;
