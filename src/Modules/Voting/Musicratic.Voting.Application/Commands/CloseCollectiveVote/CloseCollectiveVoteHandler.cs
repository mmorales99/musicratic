using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Commands.CloseCollectiveVote;

public sealed class CloseCollectiveVoteHandler(
    ICollectiveVoteSessionRepository sessionRepository,
    IVoteRepository voteRepository,
    IVotingUnitOfWork unitOfWork)
    : ICommandHandler<CloseCollectiveVoteCommand, CollectiveVoteSessionDto>
{
    public async Task<CollectiveVoteSessionDto> Handle(
        CloseCollectiveVoteCommand request,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetById(request.SessionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Collective vote session '{request.SessionId}' not found.");

        if (session.IsExpired)
        {
            session.Expire();
            sessionRepository.Update(session);
            await unitOfWork.SaveChanges(cancellationToken);

            return ToDto(session);
        }

        // Get tally for the queue entry to determine approval
        var votes = await voteRepository.GetByQueueEntryAsync(
            session.QueueEntryId,
            cancellationToken);

        var total = votes.Count;
        var upvotes = votes.Count(v => v.Value == VoteValue.Up);

        // Spec: "If ≥50% of votes are upvotes, the track is added to the queue."
        var upvotePercentage = total > 0 ? (double)upvotes / total * 100 : 0;
        var approved = upvotePercentage >= session.RequiredApprovalPercentage;

        session.Close(approved);
        sessionRepository.Update(session);
        await unitOfWork.SaveChanges(cancellationToken);

        return ToDto(session);
    }

    private static CollectiveVoteSessionDto ToDto(Domain.Entities.CollectiveVoteSession session)
    {
        return new CollectiveVoteSessionDto(
            session.Id,
            session.TenantId,
            session.QueueEntryId,
            session.ProposerId,
            session.Status,
            session.OpensAt,
            session.ExpiresAt,
            session.RequiredApprovalPercentage);
    }
}
