using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

namespace Musicratic.Social.Application.Queries.SearchHubs;

public sealed class SearchHubsQueryHandler(
    IHubDiscoveryProvider hubDiscoveryProvider,
    IHubReviewRepository hubReviewRepository) : IQueryHandler<SearchHubsQuery, SearchHubsResult>
{
    public async Task<SearchHubsResult> Handle(SearchHubsQuery request, CancellationToken ct)
    {
        var criteria = new HubSearchCriteria(
            SearchTerm: request.SearchTerm,
            IsActive: request.IsActive,
            SortBy: request.SortBy,
            Page: request.Page,
            PageSize: request.PageSize);

        var (hubs, totalCount) = await hubDiscoveryProvider.Search(criteria, ct);

        var dtos = new List<HubDiscoveryDto>(hubs.Count);

        foreach (var hub in hubs)
        {
            var (averageRating, reviewCount) = await hubReviewRepository.GetHubRating(hub.HubId, ct);

            // Per docs/09: minimum 3 reviews before rating is shown publicly
            double? displayRating = reviewCount >= 3 ? averageRating : null;

            dtos.Add(new HubDiscoveryDto(
                HubId: hub.HubId,
                Name: hub.Name,
                Description: hub.Description,
                HubType: hub.HubType,
                IsActive: hub.IsActive,
                ActiveListenerCount: hub.ActiveListenerCount,
                CurrentTrackTitle: hub.CurrentTrackTitle,
                CurrentTrackArtist: hub.CurrentTrackArtist,
                AverageRating: displayRating,
                ReviewCount: reviewCount));
        }

        return new SearchHubsResult(dtos, totalCount);
    }
}
