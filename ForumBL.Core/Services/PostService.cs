using ForumBL.Core.Abstractions.Events;
using ForumBL.Core.Abstractions.Repositories;
using ForumBL.Core.DTOs.Posts;
using ForumBL.Core.Exceptions;
using Forum.Domain.Entities;
using Forum.Domain.Enums;
using Forum.Domain.Events;
using Microsoft.Extensions.Caching.Memory;

namespace ForumBL.Core.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMemberRepository _postMemberRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostEventRepository _postEventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IForumUnitOfWork _unitOfWork;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IMemoryCache _memoryCache;

    public PostService(
        IPostRepository postRepository,
        IPostMemberRepository postMemberRepository,
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IPostEventRepository postEventRepository,
        IUserRepository userRepository,
        IForumUnitOfWork unitOfWork,
        IEventDispatcher eventDispatcher,
        IMemoryCache memoryCache)
    {
        _postRepository = postRepository;
        _postMemberRepository = postMemberRepository;
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _postEventRepository = postEventRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
        _memoryCache = memoryCache;
    }

    public async Task<PostResponse> CreateAsync(Guid currentUserId, CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppException("Title is required.");
        }

        if (request.Capacity <= 0)
        {
            throw new AppException("Capacity must be greater than zero.");
        }

        var owner = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (owner is null)
        {
            throw new AppException("User not found.", 404);
        }

        var post = new Post
        {
            Title = request.Title.Trim(),
            OwnerId = currentUserId,
            Type = request.Type,
            Capacity = request.Capacity,
            IsClosed = false
        };

        var ownerMembership = new PostMember
        {
            PostId = post.Id,
            UserId = currentUserId,
            Role = PostRole.Owner,
            Status = MembershipStatus.Accepted,
            JoinedAt = DateTime.UtcNow
        };

        await _postRepository.AddAsync(post, cancellationToken);
        await _postMemberRepository.AddAsync(ownerMembership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvent = CreatePostEvent(post.Id, currentUserId, PostEventType.PostCreated, $"Post '{post.Title}' was created.");
        await _eventDispatcher.PublishAsync(domainEvent, cancellationToken);

        var response = Map(post, 1);
        SetCache(response);
        return response;
    }

    public async Task<PostResponse> GetByIdAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue<PostResponse>(GetCacheKey(postId), out var cachedPost) && cachedPost is not null)
        {
            await EnsurePostCanBeViewedAsync(currentUserId, cachedPost.Type, postId, cancellationToken);
            return cachedPost;
        }

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new AppException("Post not found.", 404);

        await EnsurePostCanBeViewedAsync(currentUserId, post.Type, post.Id, cancellationToken);

        var acceptedMembersCount = await _postMemberRepository.CountByStatusAsync(postId, MembershipStatus.Accepted, cancellationToken);
        var response = Map(post, acceptedMembersCount);

        SetCache(response);
        return response;
    }

    public async Task<IReadOnlyList<InvitedPostResponse>> GetInvitedPostsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var invitations = await _postMemberRepository.GetPendingInvitationsForUserAsync(currentUserId, cancellationToken);

        return invitations
            .Where(x => x.Post is not null)
            .Select(x => new InvitedPostResponse
            {
                PostId = x.PostId,
                Title = x.Post!.Title,
                OwnerId = x.Post.OwnerId,
                CreatedAt = x.Post.CreatedAt,
                InvitationStatus = x.Status.ToString()
            })
            .ToList();
    }

    public async Task InviteUserToPostAsync(Guid currentUserId, Guid postId, InviteUserRequest request, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        EnsureOwner(post, currentUserId);

        if (post.IsClosed)
        {
            throw new AppException("Post is already closed.");
        }

        var invitedUser = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (invitedUser is null)
        {
            throw new AppException("Invited user not found.", 404);
        }

        var acceptedMembersCount = await _postMemberRepository.CountByStatusAsync(postId, MembershipStatus.Accepted, cancellationToken);
        if (acceptedMembersCount >= post.Capacity)
        {
            throw new AppException("Post capacity has been reached.");
        }

        var existingMembership = await _postMemberRepository.GetByPostAndUserAsync(postId, request.UserId, cancellationToken);
        if (existingMembership is not null)
        {
            throw new AppException("User already has a membership record for this post.");
        }

        var membership = new PostMember
        {
            PostId = postId,
            UserId = request.UserId,
            Role = PostRole.Member,
            Status = post.Type == PostType.Public ? MembershipStatus.Accepted : MembershipStatus.Pending,
            JoinedAt = post.Type == PostType.Public ? DateTime.UtcNow : null
        };

        await _postMemberRepository.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove(GetCacheKey(postId));

        var message = $"User '{invitedUser.Username}' was invited to post '{post.Title}'.";
        await _eventDispatcher.PublishAsync(CreatePostEvent(postId, currentUserId, PostEventType.UserInvited, message), cancellationToken);
    }

    public async Task AcceptInvitationAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        EnsurePostIsOpenForMutation(post);

        var membership = await _postMemberRepository.GetByPostAndUserAsync(postId, currentUserId, cancellationToken)
            ?? throw new AppException("Invitation not found.", 404);

        if (membership.Status != MembershipStatus.Pending)
        {
            throw new AppException("Only pending invitations can be accepted.");
        }

        var acceptedMembersCount = await _postMemberRepository.CountByStatusAsync(postId, MembershipStatus.Accepted, cancellationToken);
        if (acceptedMembersCount >= post.Capacity)
        {
            throw new AppException("Post capacity has been reached.");
        }

        membership.Status = MembershipStatus.Accepted;
        membership.JoinedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _memoryCache.Remove(GetCacheKey(postId));

        await _eventDispatcher.PublishAsync(
            CreatePostEvent(postId, currentUserId, PostEventType.UserAcceptedInvite, "An invited user accepted the invitation."),
            cancellationToken);
    }

    public async Task RejectInvitationAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        EnsurePostIsOpenForMutation(post);

        var membership = await _postMemberRepository.GetByPostAndUserAsync(postId, currentUserId, cancellationToken)
            ?? throw new AppException("Invitation not found.", 404);

        if (membership.Status != MembershipStatus.Pending)
        {
            throw new AppException("Only pending invitations can be rejected.");
        }

        membership.Status = MembershipStatus.Rejected;
        membership.JoinedAt = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.PublishAsync(
            CreatePostEvent(postId, currentUserId, PostEventType.UserRejectedInvite, "An invited user rejected the invitation."),
            cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid currentUserId, Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        EnsureOwner(post, currentUserId);
        EnsurePostIsOpenForMutation(post);

        var membership = await _postMemberRepository.GetByPostAndUserAsync(postId, userId, cancellationToken)
            ?? throw new AppException("Member not found.", 404);

        if (membership.Role == PostRole.Owner)
        {
            throw new AppException("The post owner cannot be removed.");
        }

        _postMemberRepository.Remove(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _memoryCache.Remove(GetCacheKey(postId));

        await _eventDispatcher.PublishAsync(
            CreatePostEvent(postId, currentUserId, PostEventType.UserRemoved, $"User '{userId}' was removed from the post."),
            cancellationToken);
    }

    public async Task ClosePostAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        EnsureOwner(post, currentUserId);

        if (post.IsClosed)
        {
            return;
        }

        post.IsClosed = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove(GetCacheKey(postId));

        await _eventDispatcher.PublishAsync(
            CreatePostEvent(postId, currentUserId, PostEventType.PostClosed, $"Post '{post.Title}' was closed."),
            cancellationToken);
    }

    public async Task<PostStatsResponse> GetStatsAsync(Guid currentUserId, Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await GetTrackedPostAsync(postId, cancellationToken);
        await EnsurePostCanBeViewedAsync(currentUserId, post.Type, post.Id, cancellationToken);

        var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken);
        var mostReactedComment = await _commentReactionRepository.GetMostReactedCommentAsync(postId, cancellationToken);
        var totalEvents = await _postEventRepository.CountByPostIdAsync(postId, cancellationToken);

        var firstCommenter = comments.FirstOrDefault();
        var lastCommenter = comments.LastOrDefault();

        var topCommenters = comments
            .GroupBy(x => x.UserId)
            .Select(group => new UserCommentCountDto
            {
                UserId = group.Key,
                Username = group.First().User?.Username ?? string.Empty,
                CommentCount = group.Count()
            })
            .OrderByDescending(x => x.CommentCount)
            .ThenBy(x => x.Username)
            .Take(10)
            .ToList();

        return new PostStatsResponse
        {
            TotalComments = comments.Count,
            FirstCommenter = firstCommenter?.User?.Username,
            LastCommenter = lastCommenter?.User?.Username,
            MostReactedCommentId = mostReactedComment?.CommentId,
            TotalEvents = totalEvents,
            TopCommenters = topCommenters
        };
    }

    private async Task<Post> GetTrackedPostAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new AppException("Post not found.", 404);
    }

    private async Task EnsurePostCanBeViewedAsync(Guid currentUserId, PostType postType, Guid postId, CancellationToken cancellationToken)
    {
        if (postType == PostType.Public)
        {
            return;
        }

        var membership = await _postMemberRepository.GetByPostAndUserAsync(postId, currentUserId, cancellationToken);
        if (membership?.Status != MembershipStatus.Accepted)
        {
            throw new AppException("Private posts can only be viewed by accepted members.", 403);
        }
    }

    private static void EnsureOwner(Post post, Guid currentUserId)
    {
        if (post.OwnerId != currentUserId)
        {
            throw new AppException("Only the post owner can perform this action.", 403);
        }
    }

    private static void EnsurePostIsOpenForMutation(Post post)
    {
        if (post.IsClosed)
        {
            throw new AppException("This post is closed and can no longer be modified.");
        }
    }

    private static PostResponse Map(Post post, int acceptedMembersCount)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            OwnerId = post.OwnerId,
            Type = post.Type,
            Capacity = post.Capacity,
            IsClosed = post.IsClosed,
            CreatedAt = post.CreatedAt,
            AcceptedMembersCount = acceptedMembersCount
        };
    }

    private static DomainEvent CreatePostEvent(Guid postId, Guid userId, PostEventType eventType, string message)
    {
        return new DomainEvent(eventType.ToString(), new PostDomainEventPayload(postId, userId, eventType, message));
    }

    private void SetCache(PostResponse response)
    {
        _memoryCache.Set(GetCacheKey(response.Id), response, TimeSpan.FromMinutes(5));
    }

    private static string GetCacheKey(Guid postId) => $"post:{postId}";
}
