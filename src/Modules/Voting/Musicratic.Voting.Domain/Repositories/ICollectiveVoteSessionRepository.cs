using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Entities;

namespace Musicratic.Voting.Domain.Repositories;

public interface ICollectiveVoteSessionRepository : IRepository<CollectiveVoteSession>
{
    Task<CollectiveVoteSession?> GetByQueueEntry(
        Guid queueEntryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the currently open collective vote session for this proposer in the given tenant, if any.
    /// Spec: "A visitor can have at most 1 pending collective vote proposal at a time."
    /// </summary>
    Task<CollectiveVoteSession?> GetOpenByProposer(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recently rejected collective vote session for this proposer.
    /// Spec: "After a rejected proposal, the visitor must wait 5 minutes before proposing again."
    /// </summary>
    Task<CollectiveVoteSession?> GetLastRejectedByProposer(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default);
}
