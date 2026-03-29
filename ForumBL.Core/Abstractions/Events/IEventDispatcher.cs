using Forum.Domain.Events;

namespace ForumBL.Core.Abstractions.Events;

public interface IEventDispatcher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    void Subscribe(string eventType, Func<DomainEvent, CancellationToken, Task> handler);
}
