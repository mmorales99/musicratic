using Musicratic.Hub.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Repositories;

public interface IHubMemberRepository : IRepository<HubMember>
{
    Task<HubMember?> GetMember(Guid hubId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubMember>> GetMembersByHub(Guid hubId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<HubMember> Items, int TotalCount)> GetMembersByHubPaged(
        Guid hubId, int page, int pageSize, CancellationToken cancellationToken = default);
}
