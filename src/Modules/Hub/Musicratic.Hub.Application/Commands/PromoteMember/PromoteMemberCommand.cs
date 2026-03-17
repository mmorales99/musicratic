using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.PromoteMember;

public sealed record PromoteMemberCommand(
    Guid HubId,
    Guid TargetUserId,
    HubMemberRole NewRole,
    Guid PromotedBy) : ICommand;
