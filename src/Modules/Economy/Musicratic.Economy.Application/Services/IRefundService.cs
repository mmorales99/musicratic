namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-004: Processes coin refunds when paid tracks are skipped.
/// Business rules per docs/05-voting-and-playback.md and docs/06-monetization.md:
/// - Track skipped by votes (≥65%) or owner → 50% refund (rounded down)
/// - Track played to completion → no refund
/// </summary>
public interface IRefundService
{
    /// <summary>
    /// Calculates and processes a refund for a skipped paid track.
    /// </summary>
    Task<RefundResult> ProcessSkipRefund(
        Guid hubId,
        Guid queueEntryId,
        string skipReason,
        TimeSpan playedDuration,
        TimeSpan totalDuration,
        Guid proposerUserId,
        decimal coinsSpent,
        CancellationToken cancellationToken = default);
}

public sealed record RefundResult(
    bool Refunded,
    decimal RefundAmount,
    string Reason);
