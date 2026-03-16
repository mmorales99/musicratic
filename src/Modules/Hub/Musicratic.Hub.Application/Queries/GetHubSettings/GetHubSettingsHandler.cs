using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubSettings;

public sealed class GetHubSettingsHandler(
    IHubRepository hubRepository) : IQueryHandler<GetHubSettingsQuery, HubSettings>
{
    public async Task<HubSettings> Handle(GetHubSettingsQuery request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        return hub.Settings;
    }
}
