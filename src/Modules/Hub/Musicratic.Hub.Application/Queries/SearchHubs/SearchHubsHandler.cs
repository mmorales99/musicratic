using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.SearchHubs;

public sealed class SearchHubsHandler(
    IHubRepository hubRepository) : IQueryHandler<SearchHubsQuery, PagedEnvelope<HubSummaryDto>>
{
    public async Task<PagedEnvelope<HubSummaryDto>> Handle(
        SearchHubsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await hubRepository.Search(
            request.Name,
            request.Type,
            request.Visibility,
            request.IsActive,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(h => new HubSummaryDto(
            h.Id,
            h.Name,
            h.Code,
            h.Type,
            h.IsActive,
            h.Members.Count)).ToList();

        return new PagedEnvelope<HubSummaryDto>(
            Success: true,
            TotalItemsInResponse: dtos.Count,
            HasMoreItems: page * pageSize < totalCount,
            Items: dtos,
            Audit: new AuditInfo(DateTime.UtcNow));
    }
}
