namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-005: Coin pricing engine per docs/06-monetization.md.
/// base_cost = floor(duration_seconds / 60), minimum 1 coin
/// final_cost = floor(base_cost × hotness_multiplier)
/// </summary>
public interface ICoinPricingEngine
{
    CoinPrice CalculatePrice(int trackDurationSeconds, double hubHotnessScore);
}

public sealed record CoinPrice(int BaseCost, double Multiplier, int FinalCost);
