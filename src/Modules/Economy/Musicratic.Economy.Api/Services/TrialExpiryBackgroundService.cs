using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Musicratic.Economy.Application;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Api.Services;

/// <summary>
/// ECON-014: Background service that checks for expired trials daily.
/// Per docs/06-monetization.md: at day 30, hub becomes inactive.
/// Placed in Api layer for access to Microsoft.Extensions.Hosting.
/// </summary>
public sealed class TrialExpiryBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TrialExpiryBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Trial expiry background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredTrials(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing expired trials");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredTrials(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var subscriptionRepository = scope.ServiceProvider
            .GetRequiredService<ISubscriptionRepository>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IEconomyUnitOfWork>();

        var expiredTrials = await subscriptionRepository.GetExpiredTrials(cancellationToken);

        if (expiredTrials.Count == 0) return;

        foreach (var subscription in expiredTrials)
        {
            subscription.Deactivate();
            subscriptionRepository.Update(subscription);

            logger.LogInformation(
                "Deactivated expired trial for hub {HubId}, trial ended at {TrialEndsAt}",
                subscription.HubId, subscription.TrialEndsAt);
        }

        await unitOfWork.SaveChanges(cancellationToken);
        logger.LogInformation("Deactivated {Count} expired trials", expiredTrials.Count);
    }
}
