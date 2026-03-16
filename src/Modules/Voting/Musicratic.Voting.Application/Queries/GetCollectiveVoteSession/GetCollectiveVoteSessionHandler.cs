using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Queries.GetCollectiveVoteSession;

public sealed class GetCollectiveVoteSessionHandler(
    ICollectiveVoteSessionRepository sessionRepository)
    : IQueryHandler<GetCollectiveVoteSessionQuery, CollectiveVoteSessionDto?>
{
    public async Task<CollectiveVoteSessionDto?> Handle(
        GetCollectiveVoteSessionQuery request,
        CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetById(
            request.SessionId,
            cancellationToken);

        if (session is null)
            return null;

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
