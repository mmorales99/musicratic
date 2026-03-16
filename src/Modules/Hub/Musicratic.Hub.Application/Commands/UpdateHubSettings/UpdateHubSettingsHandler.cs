using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateHubSettings;

public sealed class UpdateHubSettingsHandler(
    IHubRepository hubRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateHubSettingsCommand>
{
    public async Task Handle(UpdateHubSettingsCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var current = hub.Settings;

        var newSettings = current with
        {
            AllowProposals = request.AllowProposals ?? current.AllowProposals,
            AutoSkipThreshold = request.AutoSkipThreshold ?? current.AutoSkipThreshold,
            VotingWindowSeconds = request.VotingWindowSeconds ?? current.VotingWindowSeconds,
            MaxQueueSize = request.MaxQueueSize ?? current.MaxQueueSize,
            AllowedProviders = request.AllowedProviders ?? current.AllowedProviders,
            EnableLocalStorage = request.EnableLocalStorage ?? current.EnableLocalStorage,
            AdsEnabled = request.AdsEnabled ?? current.AdsEnabled
        };

        hub.UpdateSettings(newSettings);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
