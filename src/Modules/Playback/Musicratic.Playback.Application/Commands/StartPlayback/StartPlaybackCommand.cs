using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.StartPlayback;

public sealed record StartPlaybackCommand(Guid HubId) : ICommand;
