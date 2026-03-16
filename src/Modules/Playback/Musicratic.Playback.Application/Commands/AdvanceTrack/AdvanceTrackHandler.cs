using Musicratic.Playback.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.AdvanceTrack;

public sealed class AdvanceTrackHandler(
    IPlaybackOrchestrator orchestrator) : ICommandHandler<AdvanceTrackCommand>
{
    public async Task Handle(AdvanceTrackCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.AdvanceToNext(request.HubId, cancellationToken);
    }
}
