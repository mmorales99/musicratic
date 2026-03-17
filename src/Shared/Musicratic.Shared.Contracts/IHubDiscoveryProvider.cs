namespace Musicratic.Shared.Contracts;

public interface IHubDiscoveryProvider
{
    Task<(IReadOnlyList<HubDiscoveryResult> Items, int TotalCount)> Search(
        HubSearchCriteria criteria,
        CancellationToken ct);
}

public sealed record HubDiscoveryResult(
    Guid HubId,
    string Name,
    string? Description,
    string HubType,
    bool IsActive,
    int ActiveListenerCount,
    string? CurrentTrackTitle,
    string? CurrentTrackArtist);

public sealed record HubSearchCriteria(
    string? SearchTerm,
    bool? IsActive,
    string? SortBy,
    int Page,
    int PageSize);
