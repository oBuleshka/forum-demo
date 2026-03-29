using Forum.Domain.Enums;

namespace ForumBL.Core.DTOs.Posts;

public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public PostType Type { get; set; }
    public int Capacity { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AcceptedMembersCount { get; set; }
}
