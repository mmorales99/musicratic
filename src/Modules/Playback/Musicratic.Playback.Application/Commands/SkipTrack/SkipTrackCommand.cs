using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.SkipTrack;

public sealed record SkipTrackCommand(Guid HubId) : ICommand;
