namespace Musicratic.Playback.Domain.Models;

public sealed record TrackSearchResult(
    string ExternalId,
    string Title,
    string Artist,
    string? Album,
    int DurationSeconds,
    string? AlbumArtUrl);
