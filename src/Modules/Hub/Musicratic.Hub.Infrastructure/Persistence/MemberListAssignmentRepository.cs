using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class MemberListAssignmentRepository : IMemberListAssignmentRepository
{
    private readonly HubDbContext _dbContext;

    public MemberListAssignmentRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MemberListAssignment?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberListAssignment>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MemberListAssignment>> Find(
        Expression<Func<MemberListAssignment, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(MemberListAssignment entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.MemberListAssignments.AddAsync(entity, cancellationToken);
    }

    public void Update(MemberListAssignment entity)
    {
        _dbContext.MemberListAssignments.Update(entity);
    }

    public void Remove(MemberListAssignment entity)
    {
        _dbContext.MemberListAssignments.Remove(entity);
    }

    public async Task<MemberListAssignment?> GetAssignment(
        Guid memberId,
        Guid listId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .FirstOrDefaultAsync(a => a.MemberId == memberId && a.ListId == listId, cancellationToken);
    }

    public async Task<IReadOnlyList<MemberListAssignment>> GetByMember(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .Where(a => a.MemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetAssignedListIds(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberListAssignments
            .Where(a => a.MemberId == memberId)
            .Select(a => a.ListId)
            .ToListAsync(cancellationToken);
    }
}
