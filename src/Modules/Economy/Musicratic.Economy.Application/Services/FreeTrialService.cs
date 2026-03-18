using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-014: Free trial lifecycle per docs/06-monetization.md.
/// 30-day trial → prompt at day 20 → warn at day 28 → deactivate at day 30.
/// </summary>
public sealed class FreeTrialService(
    ISubscriptionRepository subscriptionRepository,
    IEconomyUnitOfWork unitOfWork) : IFreeTrialService
{
    private const int TrialDurationDays = 30;
    private const int ConversionPromptDay = 20;
    private const int ExpiryWarningDay = 28;

    public async Task<FreeTrialResult> StartTrial(
        Guid hubId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await subscriptionRepository.GetByHubId(hubId, cancellationToken);

        if (existing is not null)
        {
            if (existing.TrialEndsAt.HasValue)
                return new FreeTrialResult(false, null, "This hub already had a trial.");

            existing.StartTrial();
            subscriptionRepository.Update(existing);
        }
        else
        {
            var subscription = Subscription.Create(hubId, tenantId, Domain.Enums.SubscriptionTier.Free);
            subscription.StartTrial();
            await subscriptionRepository.Add(subscription, cancellationToken);
            existing = subscription;
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return new FreeTrialResult(true, existing.TrialEndsAt);
    }

    public async Task<TrialStatusResult> GetTrialStatus(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionRepository.GetByHubId(hubId, cancellationToken);

        if (subscription is null || !subscription.TrialEndsAt.HasValue)
        {
            return new TrialStatusResult(false, 0, false, false);
        }

        var daysRemaining = subscription.TrialDaysRemaining;
        var daysElapsed = TrialDurationDays - daysRemaining;

        // Spec: prompt conversion at day 20, warn at day 28
        var shouldPrompt = daysElapsed >= ConversionPromptDay && daysRemaining > 0;
        var shouldWarn = daysElapsed >= ExpiryWarningDay && daysRemaining > 0;

        return new TrialStatusResult(
            subscription.IsTrialActive,
            daysRemaining,
            shouldPrompt,
            shouldWarn);
    }
}
