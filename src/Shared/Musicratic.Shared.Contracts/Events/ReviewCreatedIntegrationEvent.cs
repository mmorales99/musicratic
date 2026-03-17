namespace Musicratic.Shared.Contracts.Events;

/// <summary>
/// Published by Social module when a review is created.
/// Dapr topic: {env}_social_review-created
/// Consumed by: Notification module.
/// </summary>
public sealed record ReviewCreatedIntegrationEvent(
    Guid ReviewId,
    Guid HubId,
    Guid ReviewerId,
    string ReviewerName,
    Guid TrackId,
    string TrackTitle,
    int Rating);
