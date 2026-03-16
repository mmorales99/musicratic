using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.ProcessSkipRefund;

/// <summary>
/// ECON-004: Triggered when a paid track is skipped.
/// Dapr subscription: {env}_voting_skip-triggered
/// </summary>
public sealed record ProcessSkipRefundCommand(
    Guid HubId,
    Guid QueueEntryId,
    string SkipReason,
    TimeSpan PlayedDuration,
    TimeSpan TotalDuration,
    Guid ProposerUserId,
    decimal CoinsSpent) : ICommand<ProcessSkipRefundResult>;

public sealed record ProcessSkipRefundResult(
    bool Refunded,
    decimal RefundAmount,
    string Reason);
