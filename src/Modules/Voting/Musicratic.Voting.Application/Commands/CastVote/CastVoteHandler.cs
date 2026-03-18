using Musicratic.Shared.Application;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Domain.Repositories;

namespace Musicratic.Voting.Application.Commands.CastVote;

public sealed class CastVoteHandler(
    IVoteRepository voteRepository,
    IVotingUnitOfWork unitOfWork,
    IVoteRateLimiter rateLimiter,
    IVotingWindowService votingWindowService,
    ISkipRuleEngine skipRuleEngine,
    IVoteTallyBroadcastService broadcastService,
    IVoteEventPublisher eventPublisher,
    IOwnerVoteService ownerVoteService) : ICommandHandler<CastVoteCommand, VoteDto>
{
    public async Task<VoteDto> Handle(
        CastVoteCommand request,
        CancellationToken cancellationToken)
    {
        // VOTE-010: Anti-abuse rate limiting
        if (!rateLimiter.CanVote(request.UserId, request.QueueEntryId, out var rateLimitReason))
        {
            throw new InvalidOperationException(rateLimitReason
                ?? "Rate limit exceeded.");
        }

        // VOTE-006: Reject late votes if voting window is closed
        if (!votingWindowService.IsVotingOpen(request.TenantId, request.QueueEntryId))
        {
            throw new InvalidOperationException(
                "Voting window is closed for this queue entry.");
        }

        // Spec: "One vote per user per queue entry."
        var existing = await voteRepository.GetByUserAndEntryAsync(
            request.UserId,
            request.QueueEntryId,
            cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"User {request.UserId} has already voted on queue entry {request.QueueEntryId}.");
        }

        var vote = Vote.Create(
            request.TenantId,
            request.UserId,
            request.QueueEntryId,
            request.Value);

        await voteRepository.Add(vote, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        // VOTE-010: Record vote for rate limiting
        rateLimiter.RecordVote(request.UserId, request.QueueEntryId);

        // Compute updated tally
        var tally = await ComputeTallyAsync(request.QueueEntryId, cancellationToken);

        // VOTE-009: Broadcast updated tally via WebSocket
        await broadcastService.BroadcastVoteCast(request.TenantId, tally, cancellationToken);

        // VOTE-013: Publish vote-cast event via Dapr
        await eventPublisher.PublishVoteCastAsync(
            request.TenantId, request.QueueEntryId, request.UserId, request.Value,
            cancellationToken);

        // VOTE-008: Owner priority vote — instant skip
        if (request.Value == VoteValue.Down
            && await ownerVoteService.IsOwnerWithSkipPower(request.UserId, request.TenantId, cancellationToken))
        {
            votingWindowService.CloseWindow(request.TenantId, request.QueueEntryId);

            const string ownerSkipReason = "Owner priority skip";

            await broadcastService.BroadcastSkipTriggered(
                request.TenantId, request.QueueEntryId, ownerSkipReason,
                cancellationToken);

            await eventPublisher.PublishSkipTriggeredAsync(
                request.TenantId, request.QueueEntryId,
                ownerSkipReason, tally.DownvotePercentage,
                cancellationToken);

            return new VoteDto(
                vote.Id,
                vote.TenantId,
                vote.UserId,
                vote.QueueEntryId,
                vote.Value,
                vote.CastAt);
        }

        // VOTE-007: Evaluate skip rule
        var skipDecision = skipRuleEngine.Evaluate(tally.Upvotes, tally.Downvotes);
        if (skipDecision.ShouldSkip)
        {
            votingWindowService.CloseWindow(request.TenantId, request.QueueEntryId);

            await broadcastService.BroadcastSkipTriggered(
                request.TenantId, request.QueueEntryId, skipDecision.Reason!,
                cancellationToken);

            await eventPublisher.PublishSkipTriggeredAsync(
                request.TenantId, request.QueueEntryId,
                skipDecision.Reason!, skipDecision.DownvotePercentage,
                cancellationToken);
        }

        return new VoteDto(
            vote.Id,
            vote.TenantId,
            vote.UserId,
            vote.QueueEntryId,
            vote.Value,
            vote.CastAt);
    }

    private async Task<VoteTallyDto> ComputeTallyAsync(
        Guid queueEntryId, CancellationToken cancellationToken)
    {
        var votes = await voteRepository.GetByQueueEntryAsync(queueEntryId, cancellationToken);
        var upvotes = votes.Count(v => v.Value == VoteValue.Up);
        var downvotes = votes.Count(v => v.Value == VoteValue.Down);
        var total = votes.Count;
        var upPct = total > 0 ? Math.Round((double)upvotes / total * 100, 2) : 0;
        var downPct = total > 0 ? Math.Round((double)downvotes / total * 100, 2) : 0;

        return new VoteTallyDto(queueEntryId, upvotes, downvotes, total, upPct, downPct);
    }
}
