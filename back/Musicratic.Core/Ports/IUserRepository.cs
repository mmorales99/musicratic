using Musicratic.Core.Models;

namespace Musicratic.Core.Ports;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task AddAsync(User user);
    Task RemoveAsync(User user);
}
