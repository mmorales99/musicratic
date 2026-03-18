using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.PauseHub;

public sealed class PauseHubHandler(
    IHubRepository hubRepository,
    IHubUnitOfWork unitOfWork) : ICommandHandler<PauseHubCommand>
{
    public async Task Handle(PauseHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.Pause();

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
