using Musicratic.Shared.Domain;

namespace Musicratic.Voting.Domain.Events;

public sealed record CollectiveVoteOpenedEvent(
    Guid SessionId,
    Guid TenantId,
    Guid QueueEntryId,
    Guid ProposerId,
    DateTime ExpiresAt) : DomainEvent;
