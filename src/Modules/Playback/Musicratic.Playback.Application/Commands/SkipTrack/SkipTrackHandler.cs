using Musicratic.Playback.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.SkipTrack;

public sealed class SkipTrackHandler(
    IPlaybackOrchestrator orchestrator) : ICommandHandler<SkipTrackCommand>
{
    public async Task Handle(SkipTrackCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.SkipCurrent(request.HubId, cancellationToken);
    }
}
