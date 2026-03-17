using Musicratic.Shared.Contracts;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Infrastructure.Services;

/// <summary>
/// VOTE-008: Checks if a user holds a role with instant-skip power (List Owner or Hub Manager).
/// Spec: "Owner downvote = instant skip (for tracks in assigned lists)" — docs/07-user-roles.md.
/// </summary>
public sealed class OwnerVoteService(
    IHubMemberRoleProvider hubMemberRoleProvider) : IOwnerVoteService
{
    private static readonly HashSet<string> SkipPowerRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "SubListOwner",
        "SubHubManager",
        "SuperOwner"
    };

    public async Task<bool> IsOwnerWithSkipPower(Guid userId, Guid hubId, CancellationToken ct)
    {
        var roleInfo = await hubMemberRoleProvider.GetMemberRole(hubId, userId, ct);
        return roleInfo is not null && SkipPowerRoles.Contains(roleInfo.Role);
    }
}
