using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Events;

namespace Musicratic.Voting.Domain.Entities;

public sealed class CollectiveVoteSession : BaseEntity, ITenantScoped
{
    private const int DefaultVotingWindowMinutes = 2;
    private const double DefaultApprovalPercentage = 50.0;

    public Guid TenantId { get; private set; }

    public Guid QueueEntryId { get; private set; }

    public Guid ProposerId { get; private set; }

    public CollectiveVoteStatus Status { get; private set; }

    public DateTime OpensAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public double RequiredApprovalPercentage { get; private set; }

    public bool IsExpired => Status == CollectiveVoteStatus.Open && DateTime.UtcNow >= ExpiresAt;

    private CollectiveVoteSession() { }

    public static CollectiveVoteSession Create(
        Guid tenantId,
        Guid queueEntryId,
        Guid proposerId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        if (queueEntryId == Guid.Empty)
            throw new ArgumentException("Queue entry ID is required.", nameof(queueEntryId));

        if (proposerId == Guid.Empty)
            throw new ArgumentException("Proposer ID is required.", nameof(proposerId));

        var now = DateTime.UtcNow;

        var session = new CollectiveVoteSession
        {
            TenantId = tenantId,
            QueueEntryId = queueEntryId,
            ProposerId = proposerId,
            Status = CollectiveVoteStatus.Open,
            OpensAt = now,
            ExpiresAt = now.AddMinutes(DefaultVotingWindowMinutes),
            RequiredApprovalPercentage = DefaultApprovalPercentage
        };

        session.AddDomainEvent(new CollectiveVoteOpenedEvent(
            session.Id,
            session.TenantId,
            session.QueueEntryId,
            session.ProposerId,
            session.ExpiresAt));

        return session;
    }

    public void Close(bool approved)
    {
        if (Status != CollectiveVoteStatus.Open)
            throw new InvalidOperationException(
                $"Cannot close a collective vote session with status '{Status}'.");

        Status = approved
            ? CollectiveVoteStatus.Approved
            : CollectiveVoteStatus.Rejected;

        AddDomainEvent(new CollectiveVoteClosedEvent(
            Id,
            TenantId,
            QueueEntryId,
            ProposerId,
            Status));
    }

    public void Expire()
    {
        if (Status != CollectiveVoteStatus.Open)
            throw new InvalidOperationException(
                $"Cannot expire a collective vote session with status '{Status}'.");

        Status = CollectiveVoteStatus.Expired;

        AddDomainEvent(new CollectiveVoteClosedEvent(
            Id,
            TenantId,
            QueueEntryId,
            ProposerId,
            Status));
    }
}
