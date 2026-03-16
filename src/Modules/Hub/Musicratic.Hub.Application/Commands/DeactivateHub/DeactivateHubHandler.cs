using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeactivateHub;

public sealed class DeactivateHubHandler(
    IHubRepository hubRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateHubCommand>
{
    public async Task Handle(DeactivateHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.Deactivate();

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
