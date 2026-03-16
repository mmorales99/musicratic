using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.RejectProposal;

public sealed class RejectProposalHandler(
    IQueueEntryRepository queueEntryRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RejectProposalCommand>
{
    public async Task Handle(
        RejectProposalCommand request, CancellationToken cancellationToken)
    {
        var entry = await queueEntryRepository.GetById(request.QueueEntryId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Queue entry '{request.QueueEntryId}' not found.");

        if (entry.HubId != request.HubId)
        {
            throw new InvalidOperationException(
                $"Queue entry '{request.QueueEntryId}' does not belong to hub '{request.HubId}'.");
        }

        // Owner/ListOwner authorization is validated at the API layer.

        entry.Reject();
        queueEntryRepository.Update(entry);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
