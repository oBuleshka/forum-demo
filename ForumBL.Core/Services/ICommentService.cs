using ForumBL.Core.DTOs.Comments;

namespace ForumBL.Core.Services;

public interface ICommentService
{
    Task<CommentResponse> AddCommentAsync(Guid currentUserId, Guid postId, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid currentUserId, Guid commentId, DeleteCommentRequest request, CancellationToken cancellationToken = default);
    Task ReactAsync(Guid currentUserId, Guid commentId, ReactToCommentRequest request, CancellationToken cancellationToken = default);
}
