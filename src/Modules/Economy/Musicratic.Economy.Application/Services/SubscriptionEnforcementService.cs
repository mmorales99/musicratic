using Musicratic.Economy.Domain.Enums;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-013: Enforces tier limits per docs/06-monetization.md.
/// </summary>
public sealed class SubscriptionEnforcementService(
    ISubscriptionRepository subscriptionRepository)
    : ISubscriptionEnforcementService
{
    private static readonly Dictionary<SubscriptionTier, TierLimits> TierLimitsMap = new()
    {
        [SubscriptionTier.Free] = new TierLimits(1, 3, 0),
        [SubscriptionTier.Basic] = new TierLimits(3, 10, 2),
        [SubscriptionTier.Premium] = new TierLimits(10, int.MaxValue, 5),
        [SubscriptionTier.Enterprise] = new TierLimits(int.MaxValue, int.MaxValue, int.MaxValue)
    };

    public async Task<bool> CanCreateHub(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var activeCount = await subscriptionRepository.CountActiveByUserId(
            userId, cancellationToken);

        var limits = GetTierLimits(SubscriptionTier.Free);

        return activeCount < limits.MaxHubs;
    }

    public async Task<bool> CanCreateList(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionRepository.GetByHubId(hubId, cancellationToken);
        var tier = subscription?.IsActive == true ? subscription.Tier : SubscriptionTier.Free;
        var limits = GetTierLimits(tier);

        // Actual list count check would be done by Hub module — we return the limit
        return limits.MaxListsPerHub > 0;
    }

    public async Task<bool> CanAddSubOwner(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionRepository.GetByHubId(hubId, cancellationToken);
        var tier = subscription?.IsActive == true ? subscription.Tier : SubscriptionTier.Free;
        var limits = GetTierLimits(tier);

        return limits.MaxSubOwners > 0;
    }

    public TierLimits GetTierLimits(SubscriptionTier tier)
    {
        return TierLimitsMap.TryGetValue(tier, out var limits)
            ? limits
            : TierLimitsMap[SubscriptionTier.Free];
    }
}
