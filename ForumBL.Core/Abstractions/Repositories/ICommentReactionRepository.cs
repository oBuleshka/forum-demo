using Forum.Domain.Entities;

namespace ForumBL.Core.Abstractions.Repositories;

public interface ICommentReactionRepository
{
    Task<CommentReaction?> GetByCommentAndUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentReaction?> GetMostReactedCommentAsync(Guid postId, CancellationToken cancellationToken = default);
    Task AddAsync(CommentReaction reaction, CancellationToken cancellationToken = default);
}
