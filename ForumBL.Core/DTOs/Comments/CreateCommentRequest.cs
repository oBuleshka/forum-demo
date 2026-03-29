namespace ForumBL.Core.DTOs.Comments;

public class CreateCommentRequest
{
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
