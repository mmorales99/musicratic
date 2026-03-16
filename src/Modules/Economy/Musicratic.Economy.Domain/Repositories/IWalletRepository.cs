using Musicratic.Economy.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Repositories;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByUserAndTenant(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
