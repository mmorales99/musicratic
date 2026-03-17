using Musicratic.Shared.Contracts.DTOs;

namespace Musicratic.Shared.Contracts;

public interface IHubMemberRoleProvider
{
    Task<HubMemberRoleInfo?> GetMemberRole(Guid hubId, Guid userId, CancellationToken ct);
}
