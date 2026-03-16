using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Repositories;

public interface IQueueEntryRepository : IRepository<QueueEntry>
{
    Task<IReadOnlyList<QueueEntry>> GetByHubId(Guid hubId, CancellationToken cancellationToken = default);

    Task<QueueEntry?> GetCurrentlyPlaying(Guid hubId, CancellationToken cancellationToken = default);

    Task<QueueEntry?> GetNextQueued(Guid hubId, CancellationToken cancellationToken = default);

    Task<int> GetNextPosition(Guid hubId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<QueueEntry> Items, int TotalCount)> GetByHubIdPaginated(
        Guid hubId, int skip, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QueueEntry>> GetPendingByProposer(
        Guid hubId, Guid proposerId, CancellationToken cancellationToken = default);

    Task<int> GetQueuedCountBySource(
        Guid hubId, QueueEntrySource source, CancellationToken cancellationToken = default);
}
