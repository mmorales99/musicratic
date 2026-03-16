using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.BulkAddListTracks;

public sealed class BulkAddListTracksHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<BulkAddListTracksCommand>
{
    public async Task Handle(BulkAddListTracksCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        foreach (var trackId in request.TrackIds)
            list.AddTrack(trackId);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
