using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.CreateList;

public sealed class CreateListHandler(
    IHubRepository hubRepository,
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateListCommand, Guid>
{
    public async Task<Guid> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var list = Domain.Entities.List.Create(
            hub.Id,
            hub.TenantId,
            request.Name,
            request.OwnerId,
            request.PlayMode);

        await listRepository.Add(list, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return list.Id;
    }
}
