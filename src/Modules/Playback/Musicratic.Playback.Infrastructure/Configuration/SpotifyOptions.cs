namespace Musicratic.Playback.Infrastructure.Configuration;

public sealed class SpotifyOptions
{
    public const string SectionName = "Spotify";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.spotify.com/v1";

    public string TokenUrl { get; set; } = "https://accounts.spotify.com/api/token";
}
