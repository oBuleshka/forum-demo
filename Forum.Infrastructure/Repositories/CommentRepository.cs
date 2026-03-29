using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;
using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ForumDbContext _dbContext;

    public CommentRepository(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<int> CountByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments.CountAsync(x => x.PostId == postId, cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.PostId == postId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments.AddAsync(comment, cancellationToken).AsTask();
    }
}
