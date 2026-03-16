using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IListRepository : IRepository<Entities.List>
{
    Task<IReadOnlyList<Entities.List>> GetListsByHub(Guid hubId, CancellationToken cancellationToken = default);
}
