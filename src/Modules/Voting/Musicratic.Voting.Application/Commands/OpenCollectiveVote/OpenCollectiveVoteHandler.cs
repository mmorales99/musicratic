using MediatR;
using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Application.Queries.CheckProposalCooldown;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Commands.OpenCollectiveVote;

public sealed class OpenCollectiveVoteHandler(
    ICollectiveVoteSessionRepository sessionRepository,
    IVotingUnitOfWork unitOfWork,
    ISender mediator)
    : ICommandHandler<OpenCollectiveVoteCommand, CollectiveVoteSessionDto>
{
    public async Task<CollectiveVoteSessionDto> Handle(
        OpenCollectiveVoteCommand request,
        CancellationToken cancellationToken)
    {
        // VOTE-015: Check cooldown before creating session
        var cooldownQuery = new CheckProposalCooldownQuery(
            request.TenantId,
            request.ProposerId);

        var cooldownResult = await mediator.Send(cooldownQuery, cancellationToken);

        if (!cooldownResult.CanPropose)
        {
            throw new InvalidOperationException(cooldownResult.Reason
                ?? "Cannot propose at this time.");
        }

        var session = CollectiveVoteSession.Create(
            request.TenantId,
            request.QueueEntryId,
            request.ProposerId);

        await sessionRepository.Add(session, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

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
