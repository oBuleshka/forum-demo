using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;
using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public class PostEventRepository : IPostEventRepository
{
    private readonly ForumDbContext _dbContext;

    public PostEventRepository(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(PostEvent postEvent, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostEvents.AddAsync(postEvent, cancellationToken).AsTask();
    }

    public Task<int> CountByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostEvents.CountAsync(x => x.PostId == postId, cancellationToken);
    }
}
