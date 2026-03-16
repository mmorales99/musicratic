using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Commands.OpenCollectiveVote;

public sealed record OpenCollectiveVoteCommand(
    Guid TenantId,
    Guid QueueEntryId,
    Guid ProposerId) : ICommand<CollectiveVoteSessionDto>;
