namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Voting module when a vote is cast.
/// Dapr topic: {env}_voting_vote-cast
/// Consumed by: Analytics module.
/// </summary>
public sealed record VoteCastIntegrationEvent(
    Guid TenantId,
    Guid QueueEntryId,
    Guid UserId,
    string Value);
