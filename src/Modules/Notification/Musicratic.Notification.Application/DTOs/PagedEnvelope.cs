namespace Musicratic.Notification.Application.DTOs;

public sealed record PagedEnvelope<T>(
    bool Success,
    int TotalItemsInResponse,
    bool HasMoreItems,
    IReadOnlyList<T> Items,
    AuditInfo Audit);

public sealed record AuditInfo(DateTime Timestamp);
