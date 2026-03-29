using Forum.Domain.Entities;

namespace ForumBL.Core.Abstractions.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
}
