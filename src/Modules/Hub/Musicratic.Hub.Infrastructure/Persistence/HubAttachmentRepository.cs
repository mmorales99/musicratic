using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Repositories;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class HubAttachmentRepository : IHubAttachmentRepository
{
    private readonly HubDbContext _dbContext;

    public HubAttachmentRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HubAttachment?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubAttachments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HubAttachment>> GetAll(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubAttachments
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HubAttachment>> Find(
        Expression<Func<HubAttachment, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubAttachments
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(HubAttachment entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.HubAttachments.AddAsync(entity, cancellationToken);
    }

    public void Update(HubAttachment entity)
    {
        _dbContext.HubAttachments.Update(entity);
    }

    public void Remove(HubAttachment entity)
    {
        _dbContext.HubAttachments.Remove(entity);
    }

    public async Task<HubAttachment?> GetActiveAttachment(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubAttachments
            .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<HubAttachment>> GetAttachmentsByHub(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubAttachments
            .Where(a => a.HubId == hubId)
            .ToListAsync(cancellationToken);
    }
}
