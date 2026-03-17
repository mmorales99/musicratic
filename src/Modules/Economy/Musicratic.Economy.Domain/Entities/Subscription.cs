using Musicratic.Economy.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Entities;

/// <summary>
/// ECON-011: Hub subscription entity per docs/06-monetization.md.
/// One subscription per hub per tenant.
/// </summary>
public sealed class Subscription : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid HubId { get; private set; }

    public SubscriptionTier Tier { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public string? StripeSubscriptionId { get; private set; }

    public DateTime? TrialEndsAt { get; private set; }

    public bool IsActive { get; private set; }

    private Subscription() { }

    public static Subscription Create(Guid hubId, Guid tenantId, SubscriptionTier tier)
    {
        if (hubId == Guid.Empty)
            throw new ArgumentException("Hub ID cannot be empty.", nameof(hubId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        return new Subscription
        {
            HubId = hubId,
            TenantId = tenantId,
            Tier = tier,
            StartedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Upgrades the subscription to a new tier.
    /// </summary>
    public void Upgrade(SubscriptionTier newTier)
    {
        if (newTier <= Tier)
            throw new InvalidOperationException(
                $"Cannot upgrade from {Tier} to {newTier}. New tier must be higher.");

        Tier = newTier;
    }

    /// <summary>
    /// Starts a 30-day free trial per docs/06-monetization.md.
    /// </summary>
    public void StartTrial()
    {
        if (TrialEndsAt.HasValue)
            throw new InvalidOperationException("Trial has already been started for this subscription.");

        Tier = SubscriptionTier.Premium;
        TrialEndsAt = DateTime.UtcNow.AddDays(30);
        IsActive = true;
    }

    public void SetStripeSubscriptionId(string stripeSubscriptionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);
        StripeSubscriptionId = stripeSubscriptionId;
    }

    public void SetExpiresAt(DateTime expiresAt)
    {
        ExpiresAt = expiresAt;
    }

    public bool IsTrialActive => TrialEndsAt.HasValue && TrialEndsAt.Value > DateTime.UtcNow && IsActive;

    public int TrialDaysRemaining => TrialEndsAt.HasValue
        ? Math.Max(0, (int)(TrialEndsAt.Value - DateTime.UtcNow).TotalDays)
        : 0;
}
