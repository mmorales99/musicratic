using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record HubResumedEvent(Guid HubId) : DomainEvent;
