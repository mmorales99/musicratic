using Musicratic.Playback.Application.DTOs;

namespace Musicratic.Playback.Application.Services;

public interface IQueueBroadcastService
{
    Task BroadcastNowPlaying(
        Guid hubId,
        NowPlayingDto nowPlaying,
        CancellationToken cancellationToken = default);

    Task BroadcastQueueUpdate(
        Guid hubId,
        IReadOnlyList<QueueEntryDto> entries,
        CancellationToken cancellationToken = default);

    Task BroadcastTrackEnded(
        Guid hubId,
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task BroadcastTrackSkipped(
        Guid hubId,
        Guid trackId,
        string reason,
        CancellationToken cancellationToken = default);
}
