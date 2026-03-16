using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ResumeHub;

public sealed class ResumeHubHandler(
    IHubRepository hubRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ResumeHubCommand>
{
    public async Task Handle(ResumeHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        hub.Resume();

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
