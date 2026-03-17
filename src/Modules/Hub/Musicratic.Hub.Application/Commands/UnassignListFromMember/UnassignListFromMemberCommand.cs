using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UnassignListFromMember;

public sealed record UnassignListFromMemberCommand(
    Guid HubId,
    Guid TargetUserId,
    Guid ListId) : ICommand;
