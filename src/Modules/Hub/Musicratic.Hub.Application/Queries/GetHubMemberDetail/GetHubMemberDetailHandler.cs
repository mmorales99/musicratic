using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubMemberDetail;

public sealed class GetHubMemberDetailHandler(
    IHubMemberRepository memberRepository) : IQueryHandler<GetHubMemberDetailQuery, HubMemberDetailDto?>
{
    public async Task<HubMemberDetailDto?> Handle(
        GetHubMemberDetailQuery request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetMember(
            request.HubId, request.UserId, cancellationToken);

        if (member is null)
            return null;

        return new HubMemberDetailDto(
            member.Id,
            member.UserId,
            DisplayName: null, // Resolved at BFF or via cross-module query
            member.Role,
            member.AssignedAt,
            member.AssignedBy);
    }
}
