using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.ApproveProposal;

public sealed record ApproveProposalCommand(
    Guid HubId,
    Guid QueueEntryId,
    Guid UserId) : ICommand;
