using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.SetPlayMode;

public sealed class SetPlayModeHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetPlayModeCommand>
{
    public async Task Handle(SetPlayModeCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        list.SetPlayMode(request.PlayMode);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
