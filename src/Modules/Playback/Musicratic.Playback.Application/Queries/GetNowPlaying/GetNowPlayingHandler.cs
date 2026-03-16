using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Queries.GetNowPlaying;

public sealed class GetNowPlayingHandler(
    IQueueEntryRepository queueEntryRepository,
    ITrackRepository trackRepository) : IQueryHandler<GetNowPlayingQuery, NowPlayingDto?>
{
    public async Task<NowPlayingDto?> Handle(
        GetNowPlayingQuery request, CancellationToken cancellationToken)
    {
        var entry = await queueEntryRepository.GetCurrentlyPlaying(
            request.HubId, cancellationToken);

        if (entry is null)
            return null;

        var track = await trackRepository.GetById(entry.TrackId, cancellationToken);
        if (track is null)
            return null;

        var elapsed = entry.StartedAt.HasValue
            ? (DateTime.UtcNow - entry.StartedAt.Value).TotalSeconds
            : 0;

        return new NowPlayingDto(
            QueueEntryId: entry.Id,
            TrackId: track.Id,
            Title: track.Title,
            Artist: track.Artist,
            Album: track.Album,
            AlbumArtUrl: track.AlbumArtUrl,
            DurationSeconds: track.DurationSeconds,
            ElapsedSeconds: Math.Min(elapsed, track.DurationSeconds),
            QueuePosition: entry.Position,
            ProposerId: entry.ProposerId,
            Source: entry.Source.ToString(),
            StartedAt: entry.StartedAt!.Value);
    }
}
