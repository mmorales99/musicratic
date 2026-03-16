using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Queries.GetTally;

public sealed class GetTallyHandler(
    IVoteRepository voteRepository) : IQueryHandler<GetTallyQuery, VoteTallyDto>
{
    public async Task<VoteTallyDto> Handle(
        GetTallyQuery request,
        CancellationToken cancellationToken)
    {
        var votes = await voteRepository.GetByQueueEntryAsync(
            request.QueueEntryId,
            cancellationToken);

        var upvotes = votes.Count(v => v.Value == VoteValue.Up);
        var downvotes = votes.Count(v => v.Value == VoteValue.Down);
        var total = votes.Count;

        var upvotePercentage = total > 0 ? Math.Round((double)upvotes / total * 100, 2) : 0;
        var downvotePercentage = total > 0 ? Math.Round((double)downvotes / total * 100, 2) : 0;

        return new VoteTallyDto(
            request.QueueEntryId,
            upvotes,
            downvotes,
            total,
            upvotePercentage,
            downvotePercentage);
    }
}
