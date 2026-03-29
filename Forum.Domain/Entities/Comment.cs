namespace Forum.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeletedForAll { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Post? Post { get; set; }
    public User? User { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
}
