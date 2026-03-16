using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record MemberJoinedEvent(Guid HubId, Guid UserId, HubMemberRole Role) : DomainEvent;
