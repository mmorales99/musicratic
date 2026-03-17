using Musicratic.Economy.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Queries.GetTransactionHistory;

/// <summary>
/// ECON-006: Query for paginated wallet transaction history.
/// </summary>
public sealed record GetTransactionHistoryQuery(
    Guid UserId,
    Guid TenantId,
    int Page,
    int PageSize,
    TransactionType? TransactionType = null) : IQuery<TransactionHistoryResult>;

public sealed record TransactionHistoryResult(
    int TotalItems,
    bool HasMoreItems,
    IReadOnlyList<TransactionDto> Items);

public sealed record TransactionDto(
    Guid Id,
    decimal Amount,
    string Type,
    string Reason,
    Guid? ReferenceId,
    DateTime CreatedAt);
