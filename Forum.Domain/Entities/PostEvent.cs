using Forum.Domain.Enums;

namespace Forum.Domain.Entities;

public class PostEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public PostEventType EventType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post? Post { get; set; }
    public User? User { get; set; }
}
