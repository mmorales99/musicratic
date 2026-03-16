using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetActiveHubs;

public sealed class GetActiveHubsHandler(
    IHubRepository hubRepository) : IQueryHandler<GetActiveHubsQuery, IReadOnlyList<HubSummaryDto>>
{
    public async Task<IReadOnlyList<HubSummaryDto>> Handle(
        GetActiveHubsQuery request,
        CancellationToken cancellationToken)
    {
        var hubs = await hubRepository.GetActiveHubs(cancellationToken);

        return hubs.Select(h => new HubSummaryDto(
            h.Id,
            h.Name,
            h.Code,
            h.Type,
            h.IsActive,
            h.Members.Count)).ToList();
    }
}
