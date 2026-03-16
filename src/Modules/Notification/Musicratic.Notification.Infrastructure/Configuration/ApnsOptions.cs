namespace Musicratic.Notification.Infrastructure.Configuration;

public sealed class ApnsOptions
{
    public const string SectionName = "Notification:Apns";

    public string? TeamId { get; set; }

    public string? KeyId { get; set; }

    public string? BundleId { get; set; }

    public string? P8PrivateKey { get; set; }

    public bool UseSandbox { get; set; } = true;

    public string BaseUrl => UseSandbox
        ? "https://api.sandbox.push.apple.com"
        : "https://api.push.apple.com";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(TeamId) &&
        !string.IsNullOrWhiteSpace(KeyId) &&
        !string.IsNullOrWhiteSpace(BundleId) &&
        !string.IsNullOrWhiteSpace(P8PrivateKey);
}
