using Musicratic.Economy.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Repositories;

public interface ICoinPackageRepository : IRepository<CoinPackage>
{
    Task<IReadOnlyList<CoinPackage>> GetActivePackages(
        CancellationToken cancellationToken = default);
}
