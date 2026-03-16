using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Repositories;
using Musicratic.Voting.Infrastructure.Persistence;

namespace Musicratic.Voting.Infrastructure.Persistence;

public sealed class VoteRepository : IVoteRepository
{
    private readonly VotingDbContext _dbContext;

    public VoteRepository(VotingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Vote?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> Find(
        Expression<Func<Vote, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        Vote entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Votes.AddAsync(entity, cancellationToken);
    }

    public void Update(Vote entity)
    {
        _dbContext.Votes.Update(entity);
    }

    public void Remove(Vote entity)
    {
        _dbContext.Votes.Remove(entity);
    }

    public async Task<Vote?> GetByUserAndEntryAsync(
        Guid userId,
        Guid queueEntryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .FirstOrDefaultAsync(
                v => v.UserId == userId && v.QueueEntryId == queueEntryId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Vote>> GetByQueueEntryAsync(
        Guid queueEntryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .Where(v => v.QueueEntryId == queueEntryId)
            .ToListAsync(cancellationToken);
    }
}
