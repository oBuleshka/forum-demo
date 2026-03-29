using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;
using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly ForumDbContext _dbContext;

    public PostRepository(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        return _dbContext.Posts.AddAsync(post, cancellationToken).AsTask();
    }
}
