namespace Musicratic.Playback.Application.DTOs;

public sealed record NowPlayingDto(
    Guid QueueEntryId,
    Guid TrackId,
    string Title,
    string Artist,
    string? Album,
    string? AlbumArtUrl,
    int DurationSeconds,
    double ElapsedSeconds,
    int QueuePosition,
    Guid? ProposerId,
    string Source,
    DateTime StartedAt);
