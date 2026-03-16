namespace Musicratic.Shared.Application;

public interface IUnitOfWork
{
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}
