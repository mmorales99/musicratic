namespace Musicratic.Notification.Infrastructure.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "Notification:Smtp";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromEmail { get; init; } = "noreply@musicratic.app";

    public string FromName { get; init; } = "Musicratic";

    public bool UseSsl { get; init; } = true;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username);
}
