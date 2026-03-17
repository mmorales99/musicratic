using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Queries.SearchHubs;

public sealed record SearchHubsQuery(
    string? SearchTerm,
    bool? IsActive,
    string? SortBy,
    int Page,
    int PageSize) : IQuery<SearchHubsResult>;

public sealed record SearchHubsResult(
    IReadOnlyList<HubDiscoveryDto> Items,
    int TotalCount);

public sealed record HubDiscoveryDto(
    Guid HubId,
    string Name,
    string? Description,
    string HubType,
    bool IsActive,
    int ActiveListenerCount,
    string? CurrentTrackTitle,
    string? CurrentTrackArtist,
    double? AverageRating,
    int ReviewCount);
