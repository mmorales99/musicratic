using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IHubRepository : IRepository<Entities.Hub>
{
    Task<Entities.Hub?> GetByCode(string code, CancellationToken cancellationToken = default);

    Task<Entities.Hub?> GetByCodeWithMembers(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Entities.Hub>> GetActiveHubs(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Entities.Hub> Items, int TotalCount)> Search(
        string? name,
        HubType? type,
        HubVisibility? visibility,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
