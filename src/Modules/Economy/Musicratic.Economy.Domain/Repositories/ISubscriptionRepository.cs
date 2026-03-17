using Musicratic.Economy.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetByHubId(
        Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetExpiredTrials(
        CancellationToken cancellationToken = default);

    Task<int> CountActiveByUserId(
        Guid userId, CancellationToken cancellationToken = default);
}
