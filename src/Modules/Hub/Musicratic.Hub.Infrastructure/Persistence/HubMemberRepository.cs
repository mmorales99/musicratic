using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class HubMemberRepository : IHubMemberRepository
{
    private readonly HubDbContext _dbContext;

    public HubMemberRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HubMember?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubMembers
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HubMember>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubMembers
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HubMember>> Find(
        Expression<Func<HubMember, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubMembers
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(HubMember entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.HubMembers.AddAsync(entity, cancellationToken);
    }

    public void Update(HubMember entity)
    {
        _dbContext.HubMembers.Update(entity);
    }

    public void Remove(HubMember entity)
    {
        _dbContext.HubMembers.Remove(entity);
    }

    public async Task<HubMember?> GetMember(
        Guid hubId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubMembers
            .FirstOrDefaultAsync(m => m.HubId == hubId && m.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<HubMember>> GetMembersByHub(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubMembers
            .Where(m => m.HubId == hubId)
            .ToListAsync(cancellationToken);
    }
}
