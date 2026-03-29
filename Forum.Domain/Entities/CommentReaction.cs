using Forum.Domain.Enums;

namespace Forum.Domain.Entities;

public class CommentReaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public ReactionType Type { get; set; }

    public Comment? Comment { get; set; }
    public User? User { get; set; }
}
