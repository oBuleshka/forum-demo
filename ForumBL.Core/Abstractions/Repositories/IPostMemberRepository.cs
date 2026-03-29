using Forum.Domain.Entities;
using Forum.Domain.Enums;

namespace ForumBL.Core.Abstractions.Repositories;

public interface IPostMemberRepository
{
    Task<PostMember?> GetByPostAndUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(Guid postId, MembershipStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PostMember>> GetPendingInvitationsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PostMember postMember, CancellationToken cancellationToken = default);
    void Remove(PostMember postMember);
}
