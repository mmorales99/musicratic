using Musicratic.Auth.Domain.Entities;
using Musicratic.Shared.Domain;

namespace Musicratic.Auth.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByAuthentikSub(string sub, CancellationToken cancellationToken = default);
}
