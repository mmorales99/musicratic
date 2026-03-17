using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Musicratic.Analytics.Application.Services;
using Musicratic.Shared.Infrastructure;

namespace Musicratic.Analytics.Api.Services;

/// <summary>
/// ANLT-007: Background service that generates monthly popular proposals reports.
/// Runs on the 1st of each month at 00:00 UTC.
/// Placed in Api layer for access to Microsoft.Extensions.Hosting.
/// </summary>
public sealed class MonthlyReportBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MonthlyReportBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Monthly proposals report background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilFirstOfMonth();
            logger.LogInformation(
                "Next monthly report run in {Hours:F1} hours", delay.TotalHours);

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

        logger.LogInformation("Monthly proposals report background service stopped");
    }

    private async Task RunReports(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var reportService = scope.ServiceProvider
                .GetRequiredService<IMonthlyPopularProposalsService>();
            var eventPublisher = scope.ServiceProvider
                .GetRequiredService<IDaprEventPublisher>();

            var reports = await reportService.GenerateAllHubReports(cancellationToken);

            foreach (var report in reports)
            {
                await eventPublisher.Publish(
                    new MonthlyProposalsReportEvent(
                        report.HubId, report.GeneratedAt, report.Proposals.Count),
                    "dev_analytics_monthly-proposals-report",
                    cancellationToken);

                logger.LogInformation(
                    "Published monthly proposals report for hub {HubId}. Proposals: {Count}",
                    report.HubId, report.Proposals.Count);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running monthly proposals reports");
        }
    }

    internal static TimeSpan CalculateDelayUntilFirstOfMonth()
    {
        var now = DateTime.UtcNow;
        var firstOfNextMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(1);

        return firstOfNextMonth - now;
    }

    internal sealed record MonthlyProposalsReportEvent(
        Guid HubId, DateTime GeneratedAt, int ProposalCount);
}
