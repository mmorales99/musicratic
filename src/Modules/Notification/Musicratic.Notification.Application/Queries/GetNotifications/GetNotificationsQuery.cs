using Musicratic.Notification.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetNotifications;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? DataJson,
    DateTime? ReadAt,
    DateTime CreatedAt);

public sealed record GetNotificationsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedEnvelope<NotificationDto>>;
