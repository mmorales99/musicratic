using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;

namespace Musicratic.TestUtilities.Builders;

public sealed class TrackBuilder
{
    private MusicProvider _provider = MusicProvider.Spotify;
    private string _externalId = "spotify-" + Guid.NewGuid().ToString("N")[..8];
    private string _title = "Test Song";
    private string _artist = "Test Artist";
    private string? _album = "Test Album";
    private int _durationSeconds = 210;
    private string? _albumArtUrl;

    public TrackBuilder WithProvider(MusicProvider provider)
    {
        _provider = provider;
        return this;
    }

    public TrackBuilder WithExternalId(string externalId)
    {
        _externalId = externalId;
        return this;
    }

    public TrackBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TrackBuilder WithArtist(string artist)
    {
        _artist = artist;
        return this;
    }

    public TrackBuilder WithAlbum(string? album)
    {
        _album = album;
        return this;
    }

    public TrackBuilder WithDuration(int durationSeconds)
    {
        _durationSeconds = durationSeconds;
        return this;
    }

    public TrackBuilder WithAlbumArtUrl(string? url)
    {
        _albumArtUrl = url;
        return this;
    }

    public Track Build() => Track.Create(
        _provider, _externalId, _title, _artist, _album, _durationSeconds, _albumArtUrl);
}
