using Musicratic.Hub.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubMembers;

public sealed record GetHubMembersQuery(
    Guid HubId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedEnvelope<HubMemberDto>>;
