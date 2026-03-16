namespace Musicratic.Shared.Contracts.Events;

public sealed record HubCreatedIntegrationEvent(
    Guid HubId,
    Guid OwnerId,
    string Name);
