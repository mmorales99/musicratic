namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-005: Coin pricing formula per docs/06-monetization.md.
/// Hotness thresholds from spec:
///   0.00–0.01 → x1.0 (Normal)
///   0.01–0.05 → x1.25 (Warm)
///   0.05–0.15 → x1.5 (Hot)
///   0.15–0.30 → x2.0 (Fire)
///   > 0.30    → x2.5 (Viral)
/// </summary>
public sealed class CoinPricingEngine : ICoinPricingEngine
{
    private readonly CoinPricingOptions _options;

    public CoinPricingEngine(CoinPricingOptions? options = null)
    {
        _options = options ?? new CoinPricingOptions();
    }

    public CoinPrice CalculatePrice(int trackDurationSeconds, double hubHotnessScore)
    {
        var baseCost = Math.Max(1, trackDurationSeconds / 60);
        var multiplier = GetHotnessMultiplier(hubHotnessScore);
        var finalCost = Math.Max(1, (int)Math.Floor(baseCost * multiplier));

        return new CoinPrice(baseCost, multiplier, finalCost);
    }

    private double GetHotnessMultiplier(double hotnessScore)
    {
        if (hotnessScore > _options.ViralThreshold) return _options.ViralMultiplier;
        if (hotnessScore > _options.FireThreshold) return _options.FireMultiplier;
        if (hotnessScore > _options.HotThreshold) return _options.HotMultiplier;
        if (hotnessScore > _options.WarmThreshold) return _options.WarmMultiplier;
        return _options.NormalMultiplier;
    }
}

public sealed class CoinPricingOptions
{
    public const string SectionName = "Economy:CoinPricing";

    public double NormalMultiplier { get; set; } = 1.0;
    public double WarmMultiplier { get; set; } = 1.25;
    public double HotMultiplier { get; set; } = 1.5;
    public double FireMultiplier { get; set; } = 2.0;
    public double ViralMultiplier { get; set; } = 2.5;

    public double WarmThreshold { get; set; } = 0.01;
    public double HotThreshold { get; set; } = 0.05;
    public double FireThreshold { get; set; } = 0.15;
    public double ViralThreshold { get; set; } = 0.30;
}
