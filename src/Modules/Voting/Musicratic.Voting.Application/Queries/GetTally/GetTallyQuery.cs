using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Queries.GetTally;

public sealed record GetTallyQuery(Guid QueueEntryId) : IQuery<VoteTallyDto>;
