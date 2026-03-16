using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class HubRepository : IHubRepository
{
    private readonly HubDbContext _dbContext;

    public HubRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.Hub?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Hub>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Hub>> Find(
        Expression<Func<Domain.Entities.Hub, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(
        Domain.Entities.Hub entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Hubs.AddAsync(entity, cancellationToken);
    }

    public void Update(Domain.Entities.Hub entity)
    {
        _dbContext.Hubs.Update(entity);
    }

    public void Remove(Domain.Entities.Hub entity)
    {
        _dbContext.Hubs.Remove(entity);
    }

    public async Task<Domain.Entities.Hub?> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .FirstOrDefaultAsync(h => h.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Hub>> GetActiveHubs(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .Where(h => h.IsActive)
            .ToListAsync(cancellationToken);
    }
}
