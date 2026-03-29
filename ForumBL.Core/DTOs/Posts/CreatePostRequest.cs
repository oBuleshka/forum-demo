using Forum.Domain.Enums;

namespace ForumBL.Core.DTOs.Posts;

public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public PostType Type { get; set; }
    public int Capacity { get; set; }
}
