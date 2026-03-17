using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

namespace Musicratic.Playback.Application.Commands.ProposeTrackPaid;

public sealed class ProposeTrackPaidHandler(
    IQueueEntryRepository queueEntryRepository,
    ITrackRepository trackRepository,
    IWalletOperationProvider walletOperationProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<ProposeTrackPaidCommand, ProposalDto>
{
    public async Task<ProposalDto> Handle(
        ProposeTrackPaidCommand request, CancellationToken cancellationToken)
    {
        var track = await trackRepository.GetById(request.TrackId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Track '{request.TrackId}' not found.");

        if (request.CoinAmount <= 0)
        {
            throw new InvalidOperationException(
                "Coin amount must be greater than zero for paid proposals.");
        }

        // Spec: No limit on coin-paid proposals beyond wallet balance
        var debitResult = await walletOperationProvider.DebitCoins(
            request.ProposerId,
            request.TenantId,
            request.CoinAmount,
            $"Track proposal: {track.Title}",
            null,
            cancellationToken);

        if (!debitResult.Success)
        {
            throw new InvalidOperationException(
                debitResult.ErrorMessage ?? "Insufficient coin balance for proposal.");
        }

        var nextPosition = await queueEntryRepository.GetNextPosition(
            request.HubId, cancellationToken);

        // Coin-paid proposals go directly to queue — no collective vote needed
        var entry = QueueEntry.Create(
            tenantId: request.TenantId,
            trackId: request.TrackId,
            hubId: request.HubId,
            position: nextPosition,
            source: QueueEntrySource.CoinProposal,
            proposerId: request.ProposerId,
            costPaid: request.CoinAmount);

        await queueEntryRepository.Add(entry, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return new ProposalDto(
            QueueEntryId: entry.Id,
            TrackId: track.Id,
            Title: track.Title,
            Artist: track.Artist,
            Status: entry.Status.ToString(),
            VoteSessionId: null,
            VoteExpiresAt: null,
            RequiredApprovalPercentage: null);
    }
}
