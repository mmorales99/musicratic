using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Infrastructure.Services;

/// <summary>
/// Implements play mode track selection logic.
/// Phase 1: WeightedShuffle uses uniform random. Extensible for weighted
/// shuffle via ShuffleWeight once scoring data is available (docs/04-hub-system.md).
/// </summary>
public sealed class PlayModeService : IPlayModeService
{
    public ListTrack? GetNextTrack(List list, Guid? currentTrackId)
    {
        if (!list.Tracks.Any())
            return null;

        return list.PlayMode switch
        {
            PlayMode.Ordered => GetNextOrdered(list, currentTrackId),
            PlayMode.WeightedShuffle => GetNextWeightedShuffle(list),
            _ => throw new InvalidOperationException($"Unknown play mode: {list.PlayMode}")
        };
    }

    private static ListTrack GetNextOrdered(List list, Guid? currentTrackId)
    {
        var ordered = list.Tracks.OrderBy(t => t.Position).ToList();

        if (currentTrackId is null)
            return ordered[0];

        var currentIndex = ordered.FindIndex(t => t.TrackId == currentTrackId);

        // If not found or at end, loop back to the first track
        if (currentIndex < 0 || currentIndex + 1 >= ordered.Count)
            return ordered[0];

        return ordered[currentIndex + 1];
    }

    private static ListTrack GetNextWeightedShuffle(List list)
    {
        // Phase 1: uniform random selection.
        // TODO: Use ShuffleWeight for weighted probability (docs/04-hub-system.md formula)
        var tracks = list.Tracks.ToList();
        var index = Random.Shared.Next(tracks.Count);
        return tracks[index];
    }
}
