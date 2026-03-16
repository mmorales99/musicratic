using Musicratic.Playback.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.StartPlayback;

public sealed class StartPlaybackHandler(
    IPlaybackOrchestrator orchestrator) : ICommandHandler<StartPlaybackCommand>
{
    public async Task Handle(StartPlaybackCommand request, CancellationToken cancellationToken)
    {
        await orchestrator.StartPlayback(request.HubId, cancellationToken);
    }
}
