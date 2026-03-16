using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ReorderListTrack;

public sealed class ReorderListTrackHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ReorderListTrackCommand>
{
    public async Task Handle(ReorderListTrackCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        list.ReorderTrack(request.TrackId, request.NewPosition);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
