namespace Musicratic.Shared.Contracts.Events;

public sealed record UserAttachedIntegrationEvent(
    Guid UserId,
    Guid HubId);
