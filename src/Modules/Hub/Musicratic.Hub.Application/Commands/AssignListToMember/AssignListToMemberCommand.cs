using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AssignListToMember;

public sealed record AssignListToMemberCommand(
    Guid HubId,
    Guid TargetUserId,
    Guid ListId,
    Guid AssignedBy) : ICommand;
