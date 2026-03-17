using Musicratic.Economy.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Queries.GetTransactionHistory;

/// <summary>
/// ECON-006: Returns paginated transaction history for a user's wallet.
/// Ordered by created_at descending.
/// </summary>
public sealed class GetTransactionHistoryHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository)
    : IQueryHandler<GetTransactionHistoryQuery, TransactionHistoryResult>
{
    public async Task<TransactionHistoryResult> Handle(
        GetTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserAndTenant(
            request.UserId, request.TenantId, cancellationToken);

        if (wallet is null)
        {
            return new TransactionHistoryResult(0, false, []);
        }

        var allTransactions = await transactionRepository.GetByWalletId(
            wallet.Id, cancellationToken);

        var filtered = request.TransactionType.HasValue
            ? allTransactions.Where(t => t.Type == request.TransactionType.Value).ToList()
            : allTransactions.ToList();

        var totalItems = filtered.Count;
        var skip = (request.Page - 1) * request.PageSize;

        var items = filtered
            .Skip(skip)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.Amount,
                t.Type.ToString(),
                t.Reason,
                t.ReferenceId,
                t.CreatedAt))
            .ToList();

        var hasMore = skip + request.PageSize < totalItems;

        return new TransactionHistoryResult(totalItems, hasMore, items);
    }
}
