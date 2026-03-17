using Musicratic.Shared.Domain;

namespace Musicratic.Social.Domain.Events;

public sealed record ReviewUpdatedEvent(Guid HubId, Guid ReviewId) : DomainEvent;
