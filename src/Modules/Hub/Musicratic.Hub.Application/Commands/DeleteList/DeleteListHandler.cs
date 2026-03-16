using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeleteList;

public sealed class DeleteListHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteListCommand>
{
    public async Task Handle(DeleteListCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        listRepository.Remove(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
