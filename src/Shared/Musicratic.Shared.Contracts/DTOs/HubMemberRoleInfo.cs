namespace Musicratic.Shared.Contracts.DTOs;

public sealed record HubMemberRoleInfo(
    Guid MemberId,
    Guid UserId,
    Guid HubId,
    string Role);
