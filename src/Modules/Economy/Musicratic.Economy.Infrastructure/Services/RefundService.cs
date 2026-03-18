using Microsoft.Extensions.Logging;
using Musicratic.Economy.Application;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// ECON-004: Refund logic per docs/05-voting-and-playback.md:
/// - Track skipped by votes (≥65%) or owner → 50% refund (rounded down)
/// - Track played to completion → no refund
/// </summary>
public sealed class RefundService(
    IWalletRepository walletRepository,
    IEconomyUnitOfWork unitOfWork,
    ILogger<RefundService> logger) : IRefundService
{
    /// <summary>
    /// Spec (docs/06-monetization.md): 50% coin refund (rounded down) when
    /// track is skipped by vote threshold or owner action.
    /// </summary>
    public async Task<RefundResult> ProcessSkipRefund(
        Guid hubId,
        Guid queueEntryId,
        string skipReason,
        TimeSpan playedDuration,
        TimeSpan totalDuration,
        Guid proposerUserId,
        decimal coinsSpent,
        CancellationToken cancellationToken = default)
    {
        if (coinsSpent <= 0)
        {
            return new RefundResult(false, 0, "No coins were spent on this track.");
        }

        // Spec: 50% refund (rounded down) for any skip scenario
        var refundAmount = Math.Floor(coinsSpent * 0.5m);

        if (refundAmount <= 0)
        {
            return new RefundResult(false, 0, "Refund amount rounds to zero.");
        }

        var wallet = await walletRepository.GetByUserAndTenant(
            proposerUserId, hubId, cancellationToken);

        if (wallet is null)
        {
            logger.LogWarning(
                "Wallet not found for user {UserId} in hub {HubId} during refund for queue entry {QueueEntryId}",
                proposerUserId, hubId, queueEntryId);
            return new RefundResult(false, 0, "Wallet not found for proposer.");
        }

        var reason = $"Skip refund: {skipReason} (QueueEntry: {queueEntryId})";
        wallet.Refund(refundAmount, reason, queueEntryId);
        walletRepository.Update(wallet);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Refunded {Amount} coins to user {UserId} in hub {HubId} for queue entry {QueueEntryId}",
            refundAmount, proposerUserId, hubId, queueEntryId);

        return new RefundResult(true, refundAmount, reason);
    }
}
