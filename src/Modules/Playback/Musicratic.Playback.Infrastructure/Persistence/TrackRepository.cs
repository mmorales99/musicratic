using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Repositories;

namespace Musicratic.Playback.Infrastructure.Persistence;

public sealed class TrackRepository : ITrackRepository
{
    private readonly PlaybackDbContext _dbContext;

    public TrackRepository(PlaybackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Track?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> GetAll(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Track>> Find(
        Expression<Func<Track, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Track entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tracks.AddAsync(entity, cancellationToken);
    }

    public void Update(Track entity)
    {
        _dbContext.Tracks.Update(entity);
    }

    public void Remove(Track entity)
    {
        _dbContext.Tracks.Remove(entity);
    }

    public async Task<Track?> GetByExternalId(
        MusicProvider provider,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tracks
            .FirstOrDefaultAsync(
                t => t.Provider == provider && t.ExternalId == externalId,
                cancellationToken);
    }
}
