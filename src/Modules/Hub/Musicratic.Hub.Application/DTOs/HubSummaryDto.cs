using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record HubSummaryDto(
    Guid Id,
    string Name,
    string Code,
    HubType Type,
    bool IsActive,
    int MemberCount);
