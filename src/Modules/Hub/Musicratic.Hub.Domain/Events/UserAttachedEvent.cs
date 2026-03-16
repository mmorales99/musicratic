using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record UserAttachedEvent(Guid HubId, Guid UserId, Guid AttachmentId) : DomainEvent;
