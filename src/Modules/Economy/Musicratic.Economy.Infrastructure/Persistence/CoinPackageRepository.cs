using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class CoinPackageRepository(EconomyDbContext dbContext) : ICoinPackageRepository
{
    public async Task<CoinPackage?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.CoinPackages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CoinPackage>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CoinPackages.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CoinPackage>> Find(
        Expression<Func<CoinPackage, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CoinPackages.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task Add(CoinPackage entity, CancellationToken cancellationToken = default)
    {
        await dbContext.CoinPackages.AddAsync(entity, cancellationToken);
    }

    public void Update(CoinPackage entity)
    {
        dbContext.CoinPackages.Update(entity);
    }

    public void Remove(CoinPackage entity)
    {
        dbContext.CoinPackages.Remove(entity);
    }

    public async Task<IReadOnlyList<CoinPackage>> GetActivePackages(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CoinPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceUsd)
            .ToListAsync(cancellationToken);
    }
}
