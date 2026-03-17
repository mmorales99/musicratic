namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Social module when a hub's aggregate rating changes.
/// Dapr topic: {env}_social_hub-rated
/// </summary>
public sealed record HubRatedIntegrationEvent(
    Guid HubId,
    double NewAverageRating,
    int ReviewCount);
