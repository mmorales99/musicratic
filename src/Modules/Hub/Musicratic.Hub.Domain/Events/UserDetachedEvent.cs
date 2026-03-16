using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record UserDetachedEvent(Guid HubId, Guid UserId, Guid AttachmentId) : DomainEvent;
