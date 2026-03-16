using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class WalletRepository(EconomyDbContext dbContext) : IWalletRepository
{
    public async Task<Wallet?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Wallets
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Wallet>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Wallets.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Wallet>> Find(
        Expression<Func<Wallet, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Wallets.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(Wallet entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Wallets.AddAsync(entity, cancellationToken);
    }

    public void Update(Wallet entity)
    {
        dbContext.Wallets.Update(entity);
    }

    public void Remove(Wallet entity)
    {
        dbContext.Wallets.Remove(entity);
    }

    public async Task<Wallet?> GetByUserAndTenant(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Wallets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                w => w.UserId == userId && w.TenantId == tenantId,
                cancellationToken);
    }
}
