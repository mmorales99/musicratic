namespace Musicratic.Analytics.Api.DTOs;

/// <summary>
/// REST collection envelope per docs/10-platform-and-tech-stack.md.
/// </summary>
public sealed record TrackStatsPagedResponse(
    bool Success,
    int TotalItemsInResponse,
    bool HasMoreItems,
    IReadOnlyList<TrackStatsResponse> Items);
