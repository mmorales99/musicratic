using Musicratic.Hub.Domain.Entities;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetHubSettings;

public sealed record GetHubSettingsQuery(Guid HubId) : IQuery<HubSettings>;
