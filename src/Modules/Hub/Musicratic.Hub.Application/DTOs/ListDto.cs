using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record ListDto(
    Guid Id,
    string Name,
    Guid OwnerId,
    PlayMode PlayMode,
    int TrackCount);
