using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Queries.CheckProposalCooldown;

public sealed class CheckProposalCooldownHandler(
    ICollectiveVoteSessionRepository sessionRepository)
    : IQueryHandler<CheckProposalCooldownQuery, CooldownCheckResult>
{
    private const int CooldownMinutes = 5;

    public async Task<CooldownCheckResult> Handle(
        CheckProposalCooldownQuery request,
        CancellationToken cancellationToken)
    {
        // Spec: "A visitor can have at most 1 pending collective vote proposal at a time."
        var openSession = await sessionRepository.GetOpenByProposer(
            request.TenantId,
            request.ProposerId,
            cancellationToken);

        if (openSession is not null)
        {
            return new CooldownCheckResult(
                CanPropose: false,
                Reason: "You already have a pending collective vote proposal.",
                CooldownEndsAt: openSession.ExpiresAt);
        }

        // Spec: "After a rejected proposal, the visitor must wait 5 minutes before proposing again."
        var lastRejected = await sessionRepository.GetLastRejectedByProposer(
            request.TenantId,
            request.ProposerId,
            cancellationToken);

        if (lastRejected is not null)
        {
            var cooldownEndsAt = lastRejected.UpdatedAt.AddMinutes(CooldownMinutes);

            if (DateTime.UtcNow < cooldownEndsAt)
            {
                return new CooldownCheckResult(
                    CanPropose: false,
                    Reason: "Your last collective vote proposal was rejected. Please wait before proposing again.",
                    CooldownEndsAt: cooldownEndsAt);
            }
        }

        return new CooldownCheckResult(CanPropose: true);
    }
}
