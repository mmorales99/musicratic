using Musicratic.Hub.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHub;

public sealed record GetHubQuery(Guid HubId) : IQuery<HubDto?>;
