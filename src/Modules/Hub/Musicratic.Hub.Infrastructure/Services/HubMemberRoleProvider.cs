using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Contracts;
using Musicratic.Shared.Contracts.DTOs;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class HubMemberRoleProvider(
    IHubMemberRepository memberRepository) : IHubMemberRoleProvider
{
    public async Task<HubMemberRoleInfo?> GetMemberRole(
        Guid hubId, Guid userId, CancellationToken ct)
    {
        var member = await memberRepository.GetMember(hubId, userId, ct);

        if (member is null)
            return null;

        return new HubMemberRoleInfo(
            member.Id,
            member.UserId,
            member.HubId,
            member.Role.ToString());
    }
}
