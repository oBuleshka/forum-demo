using ForumBL.Core.DTOs.Posts;

namespace ForumBL.Core.Services;

public interface IPostService
{
    Task<PostResponse> CreateAsync(Guid currentUserId, CreatePostRequest request, CancellationToken cancellationToken = default);
    Task<PostResponse> GetByIdAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvitedPostResponse>> GetInvitedPostsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task InviteUserToPostAsync(Guid currentUserId, Guid postId, InviteUserRequest request, CancellationToken cancellationToken = default);
    Task AcceptInvitationAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default);
    Task RejectInvitationAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid currentUserId, Guid postId, Guid userId, CancellationToken cancellationToken = default);
    Task ClosePostAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default);
    Task<PostStatsResponse> GetStatsAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default);
}
