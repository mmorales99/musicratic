using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.RemoveListTrack;

public sealed class RemoveListTrackHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveListTrackCommand>
{
    public async Task Handle(RemoveListTrackCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        list.RemoveTrack(request.TrackId);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
