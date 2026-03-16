namespace Musicratic.Playback.Application.DTOs;

public sealed record QueueEntryDto(
    Guid Id,
    Guid TrackId,
    int Position,
    string Status,
    string Source,
    Guid? ProposerId,
    int CostPaid,
    string Title,
    string Artist,
    string? Album,
    string? AlbumArtUrl,
    int DurationSeconds,
    DateTime? StartedAt,
    DateTime? EndedAt);
