using Musicratic.Analytics.Domain.Entities;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-005: Shuffle weight calculation for weighted random track selection.
/// Formula: score = (upvotes - downvotes) / max(plays, 1), normalized to 0-1 range.
/// </summary>
public interface IShuffleWeightService
{
    float CalculateWeight(TrackStats trackStats);

    Task<IReadOnlyList<(Guid TrackId, float Weight)>> GetWeightedTracks(
        Guid hubId, int count, CancellationToken cancellationToken = default);
}
