using System.Collections.Concurrent;
using ForumBL.Core.Abstractions.Events;
using Forum.Domain.Events;

namespace Forum.Infrastructure.Eventing;

public class InMemoryEventDispatcher : IEventDispatcher
{
    private readonly ConcurrentDictionary<string, List<Func<DomainEvent, CancellationToken, Task>>> _handlers = new();

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(domainEvent.Type, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.ToArray())
        {
            await handler(domainEvent, cancellationToken);
        }
    }

    public void Subscribe(string eventType, Func<DomainEvent, CancellationToken, Task> handler)
    {
        _handlers.AddOrUpdate(
            eventType,
            _ => new List<Func<DomainEvent, CancellationToken, Task>> { handler },
            (_, existingHandlers) =>
            {
                existingHandlers.Add(handler);
                return existingHandlers;
            });
    }
}
