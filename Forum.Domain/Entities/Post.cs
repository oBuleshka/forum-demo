using Forum.Domain.Enums;

namespace Forum.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public PostType Type { get; set; }
    public int Capacity { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Owner { get; set; }
    public ICollection<PostMember> Members { get; set; } = new List<PostMember>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostEvent> Events { get; set; } = new List<PostEvent>();
}
