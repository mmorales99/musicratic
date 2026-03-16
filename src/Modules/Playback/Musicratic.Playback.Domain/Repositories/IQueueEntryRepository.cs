using Musicratic.Playback.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Repositories;

public interface IQueueEntryRepository : IRepository<QueueEntry>
{
    Task<IReadOnlyList<QueueEntry>> GetByHubId(Guid hubId, CancellationToken cancellationToken = default);

    Task<QueueEntry?> GetCurrentlyPlaying(Guid hubId, CancellationToken cancellationToken = default);

    Task<int> GetNextPosition(Guid hubId, CancellationToken cancellationToken = default);
}
