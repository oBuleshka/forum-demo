namespace ForumBL.Core.DTOs.Posts;

public class UserCommentCountDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int CommentCount { get; set; }
}
