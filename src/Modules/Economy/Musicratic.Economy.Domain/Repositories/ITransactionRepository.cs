using Musicratic.Economy.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IReadOnlyList<Transaction>> GetByWalletId(
        Guid walletId, CancellationToken cancellationToken = default);
}
