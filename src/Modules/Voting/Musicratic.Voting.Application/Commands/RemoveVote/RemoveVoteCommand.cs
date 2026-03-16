using Musicratic.Shared.Application;

namespace Musicratic.Voting.Application.Commands.RemoveVote;

public sealed record RemoveVoteCommand(
    Guid TenantId,
    Guid UserId,
    Guid QueueEntryId) : ICommand;
