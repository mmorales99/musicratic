using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IHubRepository : IRepository<Entities.Hub>
{
    Task<Entities.Hub?> GetByCode(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Entities.Hub>> GetActiveHubs(CancellationToken cancellationToken = default);
}
