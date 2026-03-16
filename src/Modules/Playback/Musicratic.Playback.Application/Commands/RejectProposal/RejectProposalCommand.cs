using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.RejectProposal;

public sealed record RejectProposalCommand(
    Guid HubId,
    Guid QueueEntryId,
    Guid UserId) : ICommand;
