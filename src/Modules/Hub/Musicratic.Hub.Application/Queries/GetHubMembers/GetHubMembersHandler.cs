using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubMembers;

public sealed class GetHubMembersHandler(
    IHubMemberRepository memberRepository) : IQueryHandler<GetHubMembersQuery, IReadOnlyList<HubMemberDto>>
{
    public async Task<IReadOnlyList<HubMemberDto>> Handle(
        GetHubMembersQuery request,
        CancellationToken cancellationToken)
    {
        var members = await memberRepository.GetMembersByHub(request.HubId, cancellationToken);

        return members.Select(m => new HubMemberDto(
            m.Id,
            m.UserId,
            DisplayName: null, // Resolved at BFF or via cross-module query
            m.Role,
            m.AssignedAt)).ToList();
    }
}
