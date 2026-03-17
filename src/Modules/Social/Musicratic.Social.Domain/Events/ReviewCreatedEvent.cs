using Musicratic.Shared.Domain;

namespace Musicratic.Social.Domain.Events;

public sealed record ReviewCreatedEvent(Guid HubId, Guid UserId, int Rating) : DomainEvent;
