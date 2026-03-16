using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Repositories;

namespace Musicratic.Playback.Infrastructure.Persistence;

public sealed class QueueEntryRepository : IQueueEntryRepository
{
    private readonly PlaybackDbContext _dbContext;

    public QueueEntryRepository(PlaybackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<QueueEntry?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<QueueEntry>> GetAll(CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QueueEntry>> Find(
        Expression<Func<QueueEntry, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(QueueEntry entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.QueueEntries.AddAsync(entity, cancellationToken);
    }

    public void Update(QueueEntry entity)
    {
        _dbContext.QueueEntries.Update(entity);
    }

    public void Remove(QueueEntry entity)
    {
        _dbContext.QueueEntries.Remove(entity);
    }

    public async Task<IReadOnlyList<QueueEntry>> GetByHubId(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .Where(q => q.HubId == hubId)
            .OrderBy(q => q.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<QueueEntry?> GetCurrentlyPlaying(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .FirstOrDefaultAsync(
                q => q.HubId == hubId && q.Status == QueueEntryStatus.Playing,
                cancellationToken);
    }

    public async Task<QueueEntry?> GetNextQueued(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .Where(q => q.HubId == hubId && q.Status == QueueEntryStatus.Queued)
            .OrderBy(q => q.Position)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetNextPosition(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        var maxPosition = await _dbContext.QueueEntries
            .Where(q => q.HubId == hubId)
            .MaxAsync(q => (int?)q.Position, cancellationToken);

        return (maxPosition ?? -1) + 1;
    }

    public async Task<(IReadOnlyList<QueueEntry> Items, int TotalCount)> GetByHubIdPaginated(
        Guid hubId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.QueueEntries
            .Where(q => q.HubId == hubId)
            .OrderBy(q => q.Position);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<QueueEntry>> GetPendingByProposer(
        Guid hubId, Guid proposerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .Where(q => q.HubId == hubId
                && q.ProposerId == proposerId
                && q.Status == QueueEntryStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetQueuedCountBySource(
        Guid hubId, QueueEntrySource source, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QueueEntries
            .Where(q => q.HubId == hubId
                && q.Source == source
                && q.Status == QueueEntryStatus.Queued)
            .CountAsync(cancellationToken);
    }
}
