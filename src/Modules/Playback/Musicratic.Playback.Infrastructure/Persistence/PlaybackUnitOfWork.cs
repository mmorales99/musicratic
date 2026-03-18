using Musicratic.Playback.Application;

namespace Musicratic.Playback.Infrastructure.Persistence;

public sealed class PlaybackUnitOfWork : IPlaybackUnitOfWork
{
    private readonly PlaybackDbContext _dbContext;

    public PlaybackUnitOfWork(PlaybackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
