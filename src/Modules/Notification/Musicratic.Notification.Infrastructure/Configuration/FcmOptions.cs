namespace Musicratic.Notification.Infrastructure.Configuration;

public sealed class FcmOptions
{
    public const string SectionName = "Notification:Fcm";

    public string? ProjectId { get; set; }

    public string? ServiceAccountJson { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ProjectId) &&
        !string.IsNullOrWhiteSpace(ServiceAccountJson);
}
