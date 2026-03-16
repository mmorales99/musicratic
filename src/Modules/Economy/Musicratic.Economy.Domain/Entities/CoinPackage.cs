using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Entities;

/// <summary>
/// Supra-tenant entity — not tenant-scoped.
/// Represents purchasable coin packages per docs/06-monetization.md.
/// </summary>
public sealed class CoinPackage : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public int CoinAmount { get; private set; }

    public decimal PriceUsd { get; private set; }

    public int BonusCoins { get; private set; }

    public bool IsActive { get; private set; }

    private CoinPackage() { }

    public static CoinPackage Create(
        string name,
        int coinAmount,
        decimal priceUsd,
        int bonusCoins = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (coinAmount <= 0)
            throw new ArgumentException("Coin amount must be positive.", nameof(coinAmount));

        if (priceUsd <= 0)
            throw new ArgumentException("Price must be positive.", nameof(priceUsd));

        if (bonusCoins < 0)
            throw new ArgumentException("Bonus coins cannot be negative.", nameof(bonusCoins));

        return new CoinPackage
        {
            Name = name,
            CoinAmount = coinAmount,
            PriceUsd = priceUsd,
            BonusCoins = bonusCoins,
            IsActive = true
        };
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
