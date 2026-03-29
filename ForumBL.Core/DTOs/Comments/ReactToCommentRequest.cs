using Forum.Domain.Enums;

namespace ForumBL.Core.DTOs.Comments;

public class ReactToCommentRequest
{
    public ReactionType Type { get; set; }
}
