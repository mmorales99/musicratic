using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DemoteMember;

public sealed record DemoteMemberCommand(
    Guid HubId,
    Guid TargetUserId,
    HubMemberRole NewRole,
    Guid DemotedBy) : ICommand;
