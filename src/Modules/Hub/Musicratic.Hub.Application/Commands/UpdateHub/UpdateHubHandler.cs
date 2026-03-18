using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateHub;

public sealed class UpdateHubHandler(
    IHubRepository hubRepository,
    IHubUnitOfWork unitOfWork) : ICommandHandler<UpdateHubCommand>
{
    public async Task Handle(UpdateHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.Update(request.Name, request.Visibility);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
