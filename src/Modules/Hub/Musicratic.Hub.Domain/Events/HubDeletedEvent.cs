using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubDeletedEvent(Guid HubId) : DomainEvent;
