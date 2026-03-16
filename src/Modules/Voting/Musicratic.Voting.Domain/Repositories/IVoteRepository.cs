using Musicratic.Shared.Domain;
using Musicratic.Voting.Domain.Entities;

namespace Musicratic.Voting.Domain.Repositories;

public interface IVoteRepository : IRepository<Vote>
{
    Task<Vote?> GetByUserAndEntryAsync(
        Guid userId,
        Guid queueEntryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vote>> GetByQueueEntryAsync(
        Guid queueEntryId,
        CancellationToken cancellationToken = default);
}
