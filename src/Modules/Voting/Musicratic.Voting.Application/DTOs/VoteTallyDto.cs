namespace Musicratic.Voting.Application.DTOs;

public sealed record VoteTallyDto(
    Guid QueueEntryId,
    int Upvotes,
    int Downvotes,
    int Total,
    double UpvotePercentage,
    double DownvotePercentage);
