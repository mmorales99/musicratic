namespace Musicratic.Shared.Contracts.DTOs;

public sealed record UserBasicDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl);
