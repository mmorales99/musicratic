using Musicratic.Economy.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.ProcessSkipRefund;

/// <summary>
/// ECON-004: Handles skip refund processing.
/// Calculates refund amount per docs/05-voting-and-playback.md and credits wallet.
/// </summary>
public sealed class ProcessSkipRefundHandler(
    IRefundService refundService) : ICommandHandler<ProcessSkipRefundCommand, ProcessSkipRefundResult>
{
    public async Task<ProcessSkipRefundResult> Handle(
        ProcessSkipRefundCommand request,
        CancellationToken cancellationToken)
    {
        var result = await refundService.ProcessSkipRefund(
            request.HubId,
            request.QueueEntryId,
            request.SkipReason,
            request.PlayedDuration,
            request.TotalDuration,
            request.ProposerUserId,
            request.CoinsSpent,
            cancellationToken);

        return new ProcessSkipRefundResult(
            result.Refunded,
            result.RefundAmount,
            result.Reason);
    }
}
