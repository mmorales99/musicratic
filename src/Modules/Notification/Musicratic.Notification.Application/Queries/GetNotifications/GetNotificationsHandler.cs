using Musicratic.Notification.Application.DTOs;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Queries.GetNotifications;

public sealed class GetNotificationsHandler(
    INotificationRepository notificationRepository) : IQueryHandler<GetNotificationsQuery, PagedEnvelope<NotificationDto>>
{
    public async Task<PagedEnvelope<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var (items, totalCount) = await notificationRepository.GetByUserPaginated(
            request.UserId, skip, pageSize, cancellationToken);

        var dtos = items.Select(n => new NotificationDto(
            n.Id,
            n.Type.ToString(),
            n.Title,
            n.Body,
            n.DataJson,
            n.ReadAt,
            n.CreatedAt)).ToList();

        return new PagedEnvelope<NotificationDto>(
            Success: true,
            TotalItemsInResponse: dtos.Count,
            HasMoreItems: skip + pageSize < totalCount,
            Items: dtos,
            Audit: new AuditInfo(DateTime.UtcNow));
    }
}
