using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateList;

public sealed class UpdateListHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateListCommand>
{
    public async Task Handle(UpdateListCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        list.UpdateName(request.Name);
        list.SetPlayMode(request.PlayMode);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
