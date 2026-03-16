using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Commands.CloseCollectiveVote;

public sealed record CloseCollectiveVoteCommand(Guid SessionId) : ICommand<CollectiveVoteSessionDto>;
