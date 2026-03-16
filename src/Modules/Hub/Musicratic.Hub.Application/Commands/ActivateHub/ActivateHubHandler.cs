using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ActivateHub;

public sealed class ActivateHubHandler(
    IHubRepository hubRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivateHubCommand>
{
    public async Task Handle(ActivateHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.Activate();

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
