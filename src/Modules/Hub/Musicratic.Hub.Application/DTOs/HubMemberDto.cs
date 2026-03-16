using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record HubMemberDto(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    HubMemberRole Role,
    DateTime AssignedAt);
