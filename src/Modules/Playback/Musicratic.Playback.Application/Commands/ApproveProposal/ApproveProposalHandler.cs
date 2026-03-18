using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.ApproveProposal;

public sealed class ApproveProposalHandler(
    IQueueEntryRepository queueEntryRepository,
    IPlaybackUnitOfWork unitOfWork) : ICommandHandler<ApproveProposalCommand>
{
    public async Task Handle(
        ApproveProposalCommand request, CancellationToken cancellationToken)
    {
        var entry = await queueEntryRepository.GetById(request.QueueEntryId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Queue entry '{request.QueueEntryId}' not found.");

        if (entry.HubId != request.HubId)
        {
            throw new InvalidOperationException(
                $"Queue entry '{request.QueueEntryId}' does not belong to hub '{request.HubId}'.");
        }

        // Owner/ListOwner authorization is validated at the API layer;
        // this handler trusts the caller has verified the role.

        var nextPosition = await queueEntryRepository.GetNextPosition(
            request.HubId, cancellationToken);

        entry.Approve(nextPosition);
        queueEntryRepository.Update(entry);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
