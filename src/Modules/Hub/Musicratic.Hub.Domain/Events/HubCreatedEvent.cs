using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubCreatedEvent(Guid HubId, Guid OwnerId, string Name) : DomainEvent;
