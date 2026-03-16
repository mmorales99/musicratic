namespace Musicratic.Hub.Application.DTOs;

public sealed record ListTrackDto(
    Guid Id,
    Guid TrackId,
    int Position,
    DateTime AddedAt,
    int TotalUpvotes,
    int TotalDownvotes,
    int TotalPlays,
    double ShuffleWeight);
