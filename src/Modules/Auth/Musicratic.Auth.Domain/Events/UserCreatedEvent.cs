using Musicratic.Shared.Domain;

namespace Musicratic.Auth.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId, string Email) : DomainEvent;
