using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Application.Commands.CastVote;

public sealed record CastVoteCommand(
    Guid TenantId,
    Guid UserId,
    Guid QueueEntryId,
    VoteValue Value) : ICommand<VoteDto>;
