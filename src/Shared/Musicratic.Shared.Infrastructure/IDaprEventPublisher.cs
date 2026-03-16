namespace Musicratic.Shared.Infrastructure;

public interface IDaprEventPublisher
{
    Task Publish<TEvent>(
        TEvent @event,
        string topicName,
        CancellationToken cancellationToken = default) where TEvent : class;

    Task Publish<TEvent>(
        TEvent @event,
        string topicName,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default) where TEvent : class;
}
