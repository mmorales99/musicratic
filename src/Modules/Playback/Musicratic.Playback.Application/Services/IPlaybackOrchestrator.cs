namespace Musicratic.Playback.Application.Services;

public interface IPlaybackOrchestrator
{
    Task StartPlayback(Guid hubId, CancellationToken cancellationToken = default);

    Task AdvanceToNext(Guid hubId, CancellationToken cancellationToken = default);

    Task HandleTrackEnd(Guid hubId, CancellationToken cancellationToken = default);

    Task SkipCurrent(Guid hubId, CancellationToken cancellationToken = default);
}
