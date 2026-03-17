using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubMembers;

public sealed class GetHubMembersHandler(
    IHubMemberRepository memberRepository) : IQueryHandler<GetHubMembersQuery, PagedEnvelope<HubMemberDto>>
{
    public async Task<PagedEnvelope<HubMemberDto>> Handle(
        GetHubMembersQuery request,
        CancellationToken cancellationToken)
    {
        var (members, totalCount) = await memberRepository.GetMembersByHubPaged(
            request.HubId, request.Page, request.PageSize, cancellationToken);

        var items = members.Select(m => new HubMemberDto(
            m.Id,
            m.UserId,
            DisplayName: null, // Resolved at BFF or via cross-module query
            m.Role,
            m.AssignedAt)).ToList();

        return new PagedEnvelope<HubMemberDto>(
            Success: true,
            TotalItemsInResponse: items.Count,
            HasMoreItems: request.Page * request.PageSize < totalCount,
            Items: items,
            Audit: new AuditInfo(DateTime.UtcNow));
    }
}
