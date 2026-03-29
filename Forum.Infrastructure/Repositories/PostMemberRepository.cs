using ForumBL.Core.Abstractions.Repositories;
using Forum.Data.Context;
using Forum.Domain.Entities;
using Forum.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Forum.Infrastructure.Repositories;

public class PostMemberRepository : IPostMemberRepository
{
    private readonly ForumDbContext _dbContext;

    public PostMemberRepository(ForumDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PostMember?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PostMembers.FirstOrDefaultAsync(
            x => x.PostId == postId && x.UserId == userId,
            cancellationToken);
    }

    public Task<int> CountByStatusAsync(Guid postId, MembershipStatus status, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostMembers.CountAsync(x => x.PostId == postId && x.Status == status, cancellationToken);
    }

    public async Task<IReadOnlyList<PostMember>> GetPendingInvitationsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PostMembers
            .AsNoTracking()
            .Include(x => x.Post)
            .Where(x => x.UserId == userId && x.Status == MembershipStatus.Pending)
            .OrderByDescending(x => x.Post!.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(PostMember postMember, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostMembers.AddAsync(postMember, cancellationToken).AsTask();
    }

    public void Remove(PostMember postMember)
    {
        _dbContext.PostMembers.Remove(postMember);
    }
}
