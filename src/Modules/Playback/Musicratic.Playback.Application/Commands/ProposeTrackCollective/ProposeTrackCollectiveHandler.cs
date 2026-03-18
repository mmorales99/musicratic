using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Application.Services;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.ProposeTrackCollective;

public sealed class ProposeTrackCollectiveHandler(
    IQueueEntryRepository queueEntryRepository,
    ITrackRepository trackRepository,
    ICollectiveVoteService collectiveVoteService,
    IPlaybackUnitOfWork unitOfWork) : ICommandHandler<ProposeTrackCollectiveCommand, ProposalDto>
{
    public async Task<ProposalDto> Handle(
        ProposeTrackCollectiveCommand request, CancellationToken cancellationToken)
    {
        var track = await trackRepository.GetById(request.TrackId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Track '{request.TrackId}' not found.");

        // Check cooldown — spec: 5 min cooldown after rejected proposal
        var cooldown = await collectiveVoteService.CheckCooldown(
            request.TenantId, request.ProposerId, cancellationToken);

        if (!cooldown.CanPropose)
        {
            throw new InvalidOperationException(
                cooldown.Reason ?? "Proposal cooldown is active.");
        }

        // Spec: max 1 pending collective vote proposal per user per hub
        var pendingProposals = await queueEntryRepository.GetPendingByProposer(
            request.HubId, request.ProposerId, cancellationToken);

        if (pendingProposals.Count > 0)
        {
            throw new InvalidOperationException(
                "You already have a pending collective vote proposal in this hub.");
        }

        var nextPosition = await queueEntryRepository.GetNextPosition(
            request.HubId, cancellationToken);

        var entry = QueueEntry.CreatePendingProposal(
            tenantId: request.TenantId,
            trackId: request.TrackId,
            hubId: request.HubId,
            position: nextPosition,
            proposerId: request.ProposerId);

        await queueEntryRepository.Add(entry, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        // Open collective vote session via Voting module
        var voteResult = await collectiveVoteService.OpenVoteSession(
            request.TenantId, entry.Id, request.ProposerId, cancellationToken);

        return new ProposalDto(
            QueueEntryId: entry.Id,
            TrackId: track.Id,
            Title: track.Title,
            Artist: track.Artist,
            Status: entry.Status.ToString(),
            VoteSessionId: voteResult.VoteSessionId,
            VoteExpiresAt: voteResult.ExpiresAt,
            RequiredApprovalPercentage: voteResult.RequiredApprovalPercentage);
    }
}
