namespace Musicratic.Hub.Infrastructure.Services;

public sealed class AttachmentExpiryOptions
{
    public const string SectionName = "Hub:AttachmentExpiry";

    public int IntervalMinutes { get; set; } = 5;
}
