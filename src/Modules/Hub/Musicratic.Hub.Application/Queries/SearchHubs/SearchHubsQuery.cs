using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.SearchHubs;

public sealed record SearchHubsQuery(
    string? Name,
    HubType? Type,
    HubVisibility? Visibility,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedEnvelope<HubSummaryDto>>;
