namespace Musicratic.Auth.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string DisplayName,
    string Email,
    string? AvatarUrl,
    int WalletBalance,
    DateTime CreatedAt);
