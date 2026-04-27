namespace Musicratic.Shared.Kernel;

public abstract class Entity
{
	public Guid Id { get; init; } = Guid.NewGuid();
}

public abstract class AggregateRoot : Entity
{
}
