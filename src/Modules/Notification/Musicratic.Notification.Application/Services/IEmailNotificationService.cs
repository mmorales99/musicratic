namespace Musicratic.Notification.Application.Services;

public interface IEmailNotificationService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);

    Task SendAnalyticsReportAsync(
        Guid userId,
        string toEmail,
        string reportType,
        string htmlContent,
        CancellationToken ct = default);
}
