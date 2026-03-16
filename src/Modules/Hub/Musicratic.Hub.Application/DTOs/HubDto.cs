using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record HubDto(
    Guid Id,
    string Name,
    string Code,
    HubType Type,
    Guid OwnerId,
    bool IsActive,
    HubVisibility Visibility,
    HubSettingsDto Settings,
    DateTime CreatedAt);
