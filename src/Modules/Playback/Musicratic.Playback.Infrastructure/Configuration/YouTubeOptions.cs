namespace Musicratic.Playback.Infrastructure.Configuration;

public sealed class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3";
}
