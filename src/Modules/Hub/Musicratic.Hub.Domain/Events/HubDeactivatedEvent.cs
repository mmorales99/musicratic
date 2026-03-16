using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubDeactivatedEvent(Guid HubId) : DomainEvent;
