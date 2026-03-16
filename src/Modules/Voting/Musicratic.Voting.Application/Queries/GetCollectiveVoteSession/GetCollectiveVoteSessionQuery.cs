using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Queries.GetCollectiveVoteSession;

public sealed record GetCollectiveVoteSessionQuery(Guid SessionId) : IQuery<CollectiveVoteSessionDto?>;
