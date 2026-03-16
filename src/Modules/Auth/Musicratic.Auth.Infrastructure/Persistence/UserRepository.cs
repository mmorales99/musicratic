using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Auth.Domain.Repositories;

namespace Musicratic.Auth.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAll(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> Find(
        Expression<Func<User, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(User entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(entity, cancellationToken);
    }

    public void Update(User entity)
    {
        _dbContext.Users.Update(entity);
    }

    public void Remove(User entity)
    {
        _dbContext.Users.Remove(entity);
    }

    public async Task<User?> GetByAuthentikSub(
        string authentikSub,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.AuthentikSub == authentikSub, cancellationToken);
    }
}
