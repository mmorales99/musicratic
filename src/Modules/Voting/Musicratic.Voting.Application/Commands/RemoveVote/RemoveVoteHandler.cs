using Musicratic.Shared.Application;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Commands.RemoveVote;

public sealed class RemoveVoteHandler(
    IVoteRepository voteRepository,
    IUnitOfWork unitOfWork,
    IVoteRateLimiter rateLimiter) : ICommandHandler<RemoveVoteCommand>
{
    public async Task Handle(RemoveVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await voteRepository.GetByUserAndEntryAsync(
            request.UserId,
            request.QueueEntryId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"No vote found for user {request.UserId} on queue entry {request.QueueEntryId}.");

        voteRepository.Remove(vote);
        await unitOfWork.SaveChanges(cancellationToken);

        // VOTE-010: Record the removal as a change for rate limiting
        rateLimiter.RecordVote(request.UserId, request.QueueEntryId);
    }
}
