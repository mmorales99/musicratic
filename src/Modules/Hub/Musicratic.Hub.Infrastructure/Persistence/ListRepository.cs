using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class ListRepository : IListRepository
{
    private readonly HubDbContext _dbContext;

    public ListRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lists
            .Include(l => l.Tracks)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<List>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lists
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<List>> Find(
        Expression<Func<List, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lists
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(List entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Lists.AddAsync(entity, cancellationToken);
    }

    public void Update(List entity)
    {
        _dbContext.Lists.Update(entity);
    }

    public void Remove(List entity)
    {
        _dbContext.Lists.Remove(entity);
    }

    public async Task<IReadOnlyList<List>> GetListsByHub(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lists
            .Where(l => l.HubId == hubId)
            .ToListAsync(cancellationToken);
    }
}
