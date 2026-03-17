using Musicratic.Analytics.Domain.Entities;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-005: Calculates shuffle weights based on vote/play ratios.
/// Score = (upvotes - downvotes) / max(plays, 1), normalized to 0-1.
/// See docs/05-voting-and-playback.md for shuffle behavior.
/// </summary>
public sealed class ShuffleWeightService(ITrackStatsRepository trackStatsRepository) : IShuffleWeightService
{
    public float CalculateWeight(TrackStats trackStats)
    {
        var rawScore = trackStats.CalculateScore();
        return NormalizeScore(rawScore);
    }

    public async Task<IReadOnlyList<(Guid TrackId, float Weight)>> GetWeightedTracks(
        Guid hubId, int count, CancellationToken cancellationToken = default)
    {
        var allStats = await trackStatsRepository.GetByHub(hubId, cancellationToken);

        if (allStats.Count == 0)
            return [];

        var rawScores = allStats
            .Select(s => (Stats: s, RawScore: s.CalculateScore()))
            .ToList();

        var minScore = rawScores.Min(x => x.RawScore);
        var maxScore = rawScores.Max(x => x.RawScore);

        var weighted = rawScores
            .Select(x => (x.Stats.TrackId, Weight: NormalizeToRange(x.RawScore, minScore, maxScore)))
            .OrderByDescending(x => x.Weight)
            .Take(count)
            .ToList();

        return weighted;
    }

    /// <summary>
    /// Normalizes a raw score to 0-1 range using sigmoid-like clamping.
    /// Scores typically range from -1 to +1, mapped to 0-1.
    /// </summary>
    private static float NormalizeScore(double rawScore)
    {
        var clamped = Math.Clamp(rawScore, -1.0, 1.0);
        return (float)((clamped + 1.0) / 2.0);
    }

    /// <summary>
    /// Normalizes to 0-1 within the actual min/max range of the dataset.
    /// Ensures a minimum floor weight so all tracks have some chance.
    /// </summary>
    private static float NormalizeToRange(double score, double min, double max)
    {
        const float minimumWeight = 0.05f;

        if (Math.Abs(max - min) < 0.0001)
            return 0.5f;

        var normalized = (float)((score - min) / (max - min));
        return Math.Max(normalized, minimumWeight);
    }
}
