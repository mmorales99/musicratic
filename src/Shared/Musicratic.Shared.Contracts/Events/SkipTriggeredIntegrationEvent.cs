namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Voting module when skip rule fires.
/// Dapr topic: {env}_voting_skip-triggered
/// Consumed by: Playback module (to stop current track), Economy module (refunds).
/// </summary>
public sealed record SkipTriggeredIntegrationEvent(
    Guid TenantId,
    Guid QueueEntryId,
    string Reason,
    double DownvotePercentage);
