namespace ForumBL.Core.DTOs.Posts;

public class InvitedPostResponse
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InvitationStatus { get; set; } = string.Empty;
}
