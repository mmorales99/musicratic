using Musicratic.Analytics.Domain.Enums;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-008: Hotness calculation based on concurrent plays / active hubs ratio.
/// Formula: hotness_score = hub.PeakConcurrentUsers / max(activeHubCount, 1)
/// Tier mapping: Low (0–0.3), Medium (0.3–0.7), High (>0.7)
/// </summary>
public sealed class HotnessService(
    IHubStatsRepository hubStatsRepository) : IHotnessService
{
    private const double MediumThreshold = 0.3;
    private const double HighThreshold = 0.7;

    public async Task<HotnessTier> CalculateHotness(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var hubStats = await hubStatsRepository.GetByHub(hubId, cancellationToken);

        if (hubStats is null)
            return HotnessTier.Low;

        var activeHubs = await hubStatsRepository.GetActiveHubs(cancellationToken);
        var activeHubCount = Math.Max(activeHubs.Count, 1);
        var hotnessScore = (double)hubStats.PeakConcurrentUsers / activeHubCount;

        return hotnessScore switch
        {
            > HighThreshold => HotnessTier.High,
            > MediumThreshold => HotnessTier.Medium,
            _ => HotnessTier.Low
        };
    }
}
