using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AddListTrack;

public sealed class AddListTrackHandler(
    IListRepository listRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddListTrackCommand>
{
    public async Task Handle(AddListTrackCommand request, CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        list.AddTrack(request.TrackId);

        listRepository.Update(list);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
