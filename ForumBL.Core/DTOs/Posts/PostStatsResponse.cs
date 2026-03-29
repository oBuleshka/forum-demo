namespace ForumBL.Core.DTOs.Posts;

public class PostStatsResponse
{
    public int TotalComments { get; set; }
    public string? FirstCommenter { get; set; }
    public string? LastCommenter { get; set; }
    public Guid? MostReactedCommentId { get; set; }
    public int TotalEvents { get; set; }
    public List<UserCommentCountDto> TopCommenters { get; set; } = new();
}
