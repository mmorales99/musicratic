using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Infrastructure.Configuration;

namespace Musicratic.Notification.Infrastructure.Services;

public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IOptions<SmtpOptions> options,
        ILogger<EmailNotificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("SMTP is not configured — skipping email to {ToEmail}", toEmail);
            return;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_options.FromEmail, _options.FromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_options.Host, _options.Port);
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            client.EnableSsl = _options.UseSsl;

            await client.SendMailAsync(message, ct);

            _logger.LogInformation(
                "Email sent to {ToEmail} with subject {Subject}",
                toEmail,
                subject);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(
                ex,
                "SMTP error sending email to {ToEmail}: {StatusCode}",
                toEmail,
                ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error sending email to {ToEmail}",
                toEmail);
        }
    }

    public async Task SendAnalyticsReportAsync(
        Guid userId,
        string toEmail,
        string reportType,
        string htmlContent,
        CancellationToken ct = default)
    {
        var subject = $"Musicratic — Your {reportType} Analytics Report";

        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8" /></head>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="background-color: #6C63FF; padding: 20px; border-radius: 8px 8px 0 0;">
                    <h1 style="color: #FFFFFF; margin: 0; font-size: 24px;">Musicratic</h1>
                    <p style="color: #E0DFFF; margin: 4px 0 0 0;">{reportType} Analytics Report</p>
                </div>
                <div style="border: 1px solid #E5E7EB; border-top: none; padding: 20px; border-radius: 0 0 8px 8px;">
                    {htmlContent}
                </div>
                <p style="color: #9CA3AF; font-size: 12px; margin-top: 16px; text-align: center;">
                    You received this because you have email notifications enabled in Musicratic.
                </p>
            </body>
            </html>
            """;

        _logger.LogInformation(
            "Sending {ReportType} analytics report to user {UserId} at {ToEmail}",
            reportType,
            userId,
            toEmail);

        await SendAsync(toEmail, subject, htmlBody, ct);
    }
}
