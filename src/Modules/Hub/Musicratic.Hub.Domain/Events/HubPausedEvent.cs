using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubPausedEvent(Guid HubId) : DomainEvent;
