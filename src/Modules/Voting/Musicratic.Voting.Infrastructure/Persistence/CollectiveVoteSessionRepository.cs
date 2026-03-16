using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Repositories;
using Musicratic.Voting.Infrastructure.Persistence;

namespace Musicratic.Voting.Infrastructure.Persistence;

public sealed class CollectiveVoteSessionRepository : ICollectiveVoteSessionRepository
{
    private readonly VotingDbContext _dbContext;

    public CollectiveVoteSessionRepository(VotingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CollectiveVoteSession?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CollectiveVoteSession>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CollectiveVoteSession>> Find(
        Expression<Func<CollectiveVoteSession, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        CollectiveVoteSession entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CollectiveVoteSessions.AddAsync(entity, cancellationToken);
    }

    public void Update(CollectiveVoteSession entity)
    {
        _dbContext.CollectiveVoteSessions.Update(entity);
    }

    public void Remove(CollectiveVoteSession entity)
    {
        _dbContext.CollectiveVoteSessions.Remove(entity);
    }

    public async Task<CollectiveVoteSession?> GetByQueueEntry(
        Guid queueEntryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .FirstOrDefaultAsync(s => s.QueueEntryId == queueEntryId, cancellationToken);
    }

    public async Task<CollectiveVoteSession?> GetOpenByProposer(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .FirstOrDefaultAsync(
                s => s.TenantId == tenantId
                     && s.ProposerId == proposerId
                     && s.Status == CollectiveVoteStatus.Open,
                cancellationToken);
    }

    public async Task<CollectiveVoteSession?> GetLastRejectedByProposer(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CollectiveVoteSessions
            .Where(s => s.TenantId == tenantId
                        && s.ProposerId == proposerId
                        && s.Status == CollectiveVoteStatus.Rejected)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
