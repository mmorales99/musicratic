namespace Musicratic.Shared.Domain;

public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
