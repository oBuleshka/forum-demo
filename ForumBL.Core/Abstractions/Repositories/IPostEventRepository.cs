using Forum.Domain.Entities;

namespace ForumBL.Core.Abstractions.Repositories;

public interface IPostEventRepository
{
    Task AddAsync(PostEvent postEvent, CancellationToken cancellationToken = default);
    Task<int> CountByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
}
