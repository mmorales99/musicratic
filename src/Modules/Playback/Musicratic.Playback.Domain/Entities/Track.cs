using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Entities;

public sealed class Track : BaseEntity
{
    public MusicProvider Provider { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Artist { get; private set; } = string.Empty;

    public string? Album { get; private set; }

    public int DurationSeconds { get; private set; }

    public string? AlbumArtUrl { get; private set; }

    public double Hotness { get; private set; }

    private Track() { }

    public static Track Create(
        MusicProvider provider,
        string externalId,
        string title,
        string artist,
        string? album,
        int durationSeconds,
        string? albumArtUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId, nameof(externalId));
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(artist, nameof(artist));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(durationSeconds, 0, nameof(durationSeconds));

        var track = new Track
        {
            Provider = provider,
            ExternalId = externalId,
            Title = title,
            Artist = artist,
            Album = album,
            DurationSeconds = durationSeconds,
            AlbumArtUrl = albumArtUrl,
            Hotness = 0.0
        };

        track.AddDomainEvent(new TrackCreatedEvent(track.Id, provider, externalId));

        return track;
    }

    public void UpdateHotness(double hotness)
    {
        Hotness = hotness;
    }
}
