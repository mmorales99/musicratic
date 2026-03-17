using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Musicratic.Analytics.Application.Services;
using Musicratic.Shared.Infrastructure;

namespace Musicratic.Analytics.Api.Services;

/// <summary>
/// ANLT-006: Background service that generates weekly downvoted tracks reports.
/// Runs every Sunday at 00:00 UTC.
/// Placed in Api layer for access to Microsoft.Extensions.Hosting.
/// </summary>
public sealed class WeeklyReportBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<WeeklyReportBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Weekly downvoted report background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextSunday();
            logger.LogInformation(
                "Next weekly report run in {Hours:F1} hours", delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await RunReports(stoppingToken);
        }

        logger.LogInformation("Weekly downvoted report background service stopped");
    }

    private async Task RunReports(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider
                .GetRequiredService<IWeeklyDownvotedReportService>();
            var eventPublisher = scope.ServiceProvider
                .GetRequiredService<IDaprEventPublisher>();

            var reports = await reportService.GenerateAllHubReports(cancellationToken);

            foreach (var report in reports)
            {
                await eventPublisher.Publish(
                    new WeeklyDownvotedReportEvent(
                        report.HubId, report.GeneratedAt, report.Tracks.Count),
                    "dev_analytics_weekly-downvoted-report",
                    cancellationToken);

                logger.LogInformation(
                    "Published weekly downvoted report for hub {HubId}. Tracks: {Count}",
                    report.HubId, report.Tracks.Count);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running weekly downvoted reports");
        }
    }

    internal static TimeSpan CalculateDelayUntilNextSunday()
    {
        var now = DateTime.UtcNow;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;

        if (daysUntilSunday == 0 && now.TimeOfDay > TimeSpan.Zero)
            daysUntilSunday = 7;

        var nextSunday = now.Date.AddDays(daysUntilSunday);
        return nextSunday - now;
    }

    internal sealed record WeeklyDownvotedReportEvent(
        Guid HubId, DateTime GeneratedAt, int DownvotedTrackCount);
}
