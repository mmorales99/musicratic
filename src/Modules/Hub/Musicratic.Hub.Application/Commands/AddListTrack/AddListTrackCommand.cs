using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AddListTrack;

public sealed record AddListTrackCommand(Guid ListId, Guid TrackId) : ICommand;
