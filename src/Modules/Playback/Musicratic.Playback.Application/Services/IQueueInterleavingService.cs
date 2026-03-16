using Musicratic.Playback.Domain.Enums;

namespace Musicratic.Playback.Application.Services;

public interface IQueueInterleavingService
{
    /// <summary>
    /// Calculates the optimal position for a new queue entry based on the
    /// configured list-to-proposal interleaving ratio.
    /// </summary>
    Task<int> CalculateInsertPosition(
        Guid hubId,
        QueueEntrySource source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders existing queued entries to maintain the interleaving ratio.
    /// </summary>
    Task ReorderQueue(Guid hubId, CancellationToken cancellationToken = default);
}
