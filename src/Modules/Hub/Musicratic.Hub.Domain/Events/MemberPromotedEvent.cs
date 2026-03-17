using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Events;

public sealed record MemberPromotedEvent(
    Guid HubId,
    Guid UserId,
    HubMemberRole OldRole,
    HubMemberRole NewRole,
    Guid PromotedBy) : DomainEvent;
