using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record HubMemberDetailDto(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    HubMemberRole Role,
    DateTime AssignedAt,
    Guid? AssignedBy);
