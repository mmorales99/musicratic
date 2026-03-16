using Musicratic.Playback.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Queries.GetQueue;

public sealed record GetQueueQuery(
    Guid HubId,
    int Page = 1,
    int PageSize = 20) : IQuery<GetQueueResult>;

public sealed record GetQueueResult(
    IReadOnlyList<QueueEntryDto> Items,
    int TotalItems,
    bool HasMoreItems);
