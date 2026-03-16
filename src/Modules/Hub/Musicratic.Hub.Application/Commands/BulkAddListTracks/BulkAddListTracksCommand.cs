using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.BulkAddListTracks;

public sealed record BulkAddListTracksCommand(Guid ListId, IReadOnlyList<Guid> TrackIds) : ICommand;
