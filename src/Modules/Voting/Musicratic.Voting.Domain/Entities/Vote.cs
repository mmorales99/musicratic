using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Events;

namespace Musicratic.Voting.Domain.Entities;

public sealed class Vote : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid QueueEntryId { get; private set; }

    public VoteValue Value { get; private set; }

    public DateTime CastAt { get; private set; }

    private Vote() { }

    public static Vote Create(Guid tenantId, Guid userId, Guid queueEntryId, VoteValue value)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (queueEntryId == Guid.Empty)
            throw new ArgumentException("Queue entry ID is required.", nameof(queueEntryId));

        var vote = new Vote
        {
            TenantId = tenantId,
            UserId = userId,
            QueueEntryId = queueEntryId,
            Value = value,
            CastAt = DateTime.UtcNow
        };

        vote.AddDomainEvent(new VoteCastEvent(
            vote.Id,
            vote.TenantId,
            vote.QueueEntryId,
            vote.UserId,
            vote.Value));

        return vote;
    }
}
