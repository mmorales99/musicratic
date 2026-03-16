using System.Linq.Expressions;

namespace Musicratic.Shared.Domain;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> Find(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task Add(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
