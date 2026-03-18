using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeleteHub;

public sealed class DeleteHubHandler(
    IHubRepository hubRepository,
    IHubUnitOfWork unitOfWork) : ICommandHandler<DeleteHubCommand>
{
    public async Task Handle(DeleteHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.SoftDelete();

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
