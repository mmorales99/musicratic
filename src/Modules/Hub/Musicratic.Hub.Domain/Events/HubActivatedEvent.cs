using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubActivatedEvent(Guid HubId) : DomainEvent;
