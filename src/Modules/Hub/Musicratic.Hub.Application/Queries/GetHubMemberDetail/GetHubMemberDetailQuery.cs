using Musicratic.Hub.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubMemberDetail;

public sealed record GetHubMemberDetailQuery(Guid HubId, Guid UserId) : IQuery<HubMemberDetailDto?>;
