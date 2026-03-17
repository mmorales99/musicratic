using Musicratic.Economy.Domain.Enums;

namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-013: Subscription tier enforcement per docs/06-monetization.md.
/// Validates hub/list/sub-owner limits based on subscription tier.
/// </summary>
public interface ISubscriptionEnforcementService
{
    Task<bool> CanCreateHub(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> CanCreateList(Guid hubId, CancellationToken cancellationToken = default);

    Task<bool> CanAddSubOwner(Guid hubId, CancellationToken cancellationToken = default);

    TierLimits GetTierLimits(SubscriptionTier tier);
}

/// <summary>
/// Tier limits per docs/06-monetization.md:
/// Free(1 hub, 3 lists, 0 sub-owners),
/// Basic(3, 10, 2),
/// Premium(10, unlimited, 5),
/// Enterprise(unlimited, unlimited, unlimited).
/// Use int.MaxValue for "unlimited".
/// </summary>
public sealed record TierLimits(
    int MaxHubs,
    int MaxListsPerHub,
    int MaxSubOwners);
