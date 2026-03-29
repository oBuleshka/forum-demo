namespace Forum.Domain.Events;

public class DomainEvent
{
    public DomainEvent(string type, object payload)
    {
        Type = type;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    public string Type { get; }
    public object Payload { get; }
    public DateTime CreatedAt { get; }
}
