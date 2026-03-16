using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Domain.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.CreateHub;

public sealed class CreateHubHandler(
    IHubRepository hubRepository,
    IHubCodeGenerator hubCodeGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateHubCommand, Guid>
{
    public async Task<Guid> Handle(CreateHubCommand request, CancellationToken cancellationToken)
    {
        var code = await hubCodeGenerator.Generate(request.Name, cancellationToken);

        var hub = Domain.Entities.Hub.Create(
            request.Name,
            request.Type,
            request.OwnerId,
            request.Settings,
            code);

        // Auto-add owner as SuperOwner member
        hub.AddMember(request.OwnerId, HubMemberRole.SuperOwner, assignedBy: null);

        await hubRepository.Add(hub, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return hub.Id;
    }
}
