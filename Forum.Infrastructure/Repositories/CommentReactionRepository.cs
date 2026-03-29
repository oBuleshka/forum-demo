using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;
using Forum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public class CommentReactionRepository : ICommentReactionRepository
{
    private readonly ForumDbContext _dbContext;

    public CommentReactionRepository(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentReaction?> GetByCommentAndUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommentReactions.FirstOrDefaultAsync(
            x => x.CommentId == commentId && x.UserId == userId,
            cancellationToken);
    }

    public async Task<CommentReaction?> GetMostReactedCommentAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CommentReactions
            .AsNoTracking()
            .Where(x => x.Comment != null && x.Comment.PostId == postId)
            .GroupBy(x => x.CommentId)
            .OrderByDescending(x => x.Count())
            .Select(x => x.First())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(CommentReaction reaction, CancellationToken cancellationToken = default)
    {
        return _dbContext.CommentReactions.AddAsync(reaction, cancellationToken).AsTask();
    }
}
