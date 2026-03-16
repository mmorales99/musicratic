using System.Linq.Expressions;
using Musicratic.Shared.Domain;

namespace Musicratic.TestUtilities.Fakes;

public class FakeRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly List<T> _entities = [];

    public Task<T?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = _entities.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<T> result = _entities.ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<T>> Find(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var compiled = predicate.Compile();
        IReadOnlyList<T> result = _entities.Where(compiled).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    public Task Add(T entity, CancellationToken cancellationToken = default)
    {
        _entities.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
        // In-memory: entity is already a reference, no action needed.
    }

    public void Remove(T entity)
    {
        _entities.Remove(entity);
    }
}
