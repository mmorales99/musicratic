using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Enums;
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

    public async Task<Domain.Entities.Hub?> GetByCodeWithMembers(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Code == code, cancellationToken);
    }

    public async Task<Domain.Entities.Hub?> GetByIdWithMembers(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Hub>> GetActiveHubs(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hubs
            .Where(h => h.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Domain.Entities.Hub> Items, int TotalCount)> Search(
        string? name,
        HubType? type,
        HubVisibility? visibility,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Hubs
            .Where(h => !h.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(h => h.Name.Contains(name));

        if (type.HasValue)
            query = query.Where(h => h.Type == type.Value);

        if (visibility.HasValue)
            query = query.Where(h => h.Visibility == visibility.Value);

        if (isActive.HasValue)
            query = query.Where(h => h.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(h => h.Members)
            .OrderBy(h => h.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
