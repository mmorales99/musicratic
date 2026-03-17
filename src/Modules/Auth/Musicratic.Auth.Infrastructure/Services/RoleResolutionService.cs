using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Domain.Enums;
using Musicratic.Shared.Contracts;

namespace Musicratic.Auth.Infrastructure.Services;

public sealed class RoleResolutionService(
    IHubMemberRoleProvider hubMemberRoleProvider) : IRoleResolutionService
{
    public async Task<ResolvedRole> Resolve(Guid userId, Guid hubId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return new ResolvedRole(UserRole.Anonymous, MemberId: null);

        var memberRole = await hubMemberRoleProvider.GetMemberRole(hubId, userId, ct);

        if (memberRole is null)
            return new ResolvedRole(UserRole.User, MemberId: null);

        var role = MapRole(memberRole.Role);
        return new ResolvedRole(role, memberRole.MemberId);
    }

    // Maps Hub module role strings to Auth module UserRole enum.
    // See docs/07-user-roles.md for the 5-tier accumulative role system.
    private static UserRole MapRole(string role) => role switch
    {
        "Visitor" => UserRole.Visitor,
        "SubListOwner" => UserRole.ListOwner,
        "SubHubManager" => UserRole.HubManager,
        "SuperOwner" => UserRole.HubManager,
        _ => UserRole.User
    };
}
