namespace Musicratic.Social.Application.DTOs;

public sealed record ReviewDto(
    Guid Id,
    Guid HubId,
    Guid UserId,
    int Rating,
    string? Comment,
    string? OwnerResponse,
    DateTime CreatedAt);
