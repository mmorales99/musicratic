using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class TransactionRepository(EconomyDbContext dbContext) : ITransactionRepository
{
    public async Task<Transaction?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> Find(
        Expression<Func<Transaction, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(Transaction entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Transactions.AddAsync(entity, cancellationToken);
    }

    public void Update(Transaction entity)
    {
        dbContext.Transactions.Update(entity);
    }

    public void Remove(Transaction entity)
    {
        dbContext.Transactions.Remove(entity);
    }

    public async Task<IReadOnlyList<Transaction>> GetByWalletId(
        Guid walletId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Transactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
