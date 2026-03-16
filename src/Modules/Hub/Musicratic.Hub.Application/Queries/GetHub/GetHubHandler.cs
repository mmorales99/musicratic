using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHub;

public sealed class GetHubHandler(
    IHubRepository hubRepository) : IQueryHandler<GetHubQuery, HubDto?>
{
    public async Task<HubDto?> Handle(GetHubQuery request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken);

        if (hub is null)
            return null;

        var settingsDto = new HubSettingsDto(
            hub.Settings.AllowProposals,
            hub.Settings.AutoSkipThreshold,
            hub.Settings.VotingWindowSeconds,
            hub.Settings.MaxQueueSize,
            hub.Settings.AllowedProviders,
            hub.Settings.EnableLocalStorage,
            hub.Settings.AdsEnabled);

        return new HubDto(
            hub.Id,
            hub.Name,
            hub.Code,
            hub.Type,
            hub.OwnerId,
            hub.IsActive,
            hub.Visibility,
            settingsDto,
            hub.CreatedAt);
    }
}
