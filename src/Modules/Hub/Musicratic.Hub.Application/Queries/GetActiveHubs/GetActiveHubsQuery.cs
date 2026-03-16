using Musicratic.Hub.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetActiveHubs;

public sealed record GetActiveHubsQuery : IQuery<IReadOnlyList<HubSummaryDto>>;
