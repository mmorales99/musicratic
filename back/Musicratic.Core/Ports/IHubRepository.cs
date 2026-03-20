using Musicratic.Core.Models;

namespace Musicratic.Core.Ports;

public interface IHubRepository
{
    Task<Hub?> GetByIdAsync(string id);
    Task AddAsync(Hub hub);
    Task SaveAsync(Hub hub);
}
