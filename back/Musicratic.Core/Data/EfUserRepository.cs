using Microsoft.EntityFrameworkCore;
using Musicratic.Core.Models;
using Musicratic.Core.Ports;

namespace Musicratic.Core.Data;

public class EfUserRepository(MusicraticDbContext db) : IUserRepository
{
    public async Task<IEnumerable<User>> GetAllAsync() =>
        (await db.Users.ToListAsync()).Select(Mapper.ToDomain);

    public async Task<User?> GetByIdAsync(string id)
    {
        var entity = await db.Users.FindAsync(id);
        return entity is null ? null : Mapper.ToDomain(entity);
    }

    public async Task AddAsync(User user)
    {
        if (await db.Users.FindAsync(user.Id) is null)
        {
            db.Users.Add(Mapper.ToEntity(user));
            await db.SaveChangesAsync();
        }
    }

    public async Task RemoveAsync(User user)
    {
        var entity = await db.Users.FindAsync(user.Id);
        if (entity is not null)
        {
            db.Users.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
