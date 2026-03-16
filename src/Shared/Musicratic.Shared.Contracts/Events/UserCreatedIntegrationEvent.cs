namespace Musicratic.Shared.Contracts.Events;

public sealed record UserCreatedIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName);
