namespace Forum.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Post> OwnedPosts { get; set; } = new List<Post>();
    public ICollection<PostMember> PostMemberships { get; set; } = new List<PostMember>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    public ICollection<PostEvent> PostEvents { get; set; } = new List<PostEvent>();
}
