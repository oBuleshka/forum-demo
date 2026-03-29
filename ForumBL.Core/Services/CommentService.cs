using ForumBL.Core.Abstractions.Events;
using ForumBL.Core.Abstractions.Repositories;
using ForumBL.Core.DTOs.Comments;
using ForumBL.Core.Exceptions;
using Forum.Domain.Entities;
using Forum.Domain.Enums;
using Forum.Domain.Events;

namespace ForumBL.Core.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentReactionRepository _commentReactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly IPostMemberRepository _postMemberRepository;
    private readonly IForumUnitOfWork _unitOfWork;
    private readonly IEventDispatcher _eventDispatcher;

    public CommentService(
        ICommentRepository commentRepository,
        ICommentReactionRepository commentReactionRepository,
        IPostRepository postRepository,
        IPostMemberRepository postMemberRepository,
        IForumUnitOfWork unitOfWork,
        IEventDispatcher eventDispatcher)
    {
        _commentRepository = commentRepository;
        _commentReactionRepository = commentReactionRepository;
        _postRepository = postRepository;
        _postMemberRepository = postMemberRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<CommentResponse> AddCommentAsync(Guid currentUserId, Guid postId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new AppException("Comment content is required.");
        }

        var post = await _postRepository.GetByIdAsync(postId, cancellationToken)
            ?? throw new AppException("Post not found.", 404);

        if (post.IsClosed)
        {
            throw new AppException("Comments are closed for this post.");
        }

        await EnsureCanParticipateAsync(post, currentUserId, cancellationToken);

        if (request.ParentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent is null || parent.PostId != postId)
            {
                throw new AppException("Parent comment was not found in this post.");
            }
        }

        var comment = new Comment
        {
            PostId = postId,
            UserId = currentUserId,
            ParentId = request.ParentId,
            Content = request.Content.Trim()
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var eventType = request.ParentId.HasValue ? PostEventType.CommentReplied : PostEventType.CommentAdded;
        await _eventDispatcher.PublishAsync(
            new DomainEvent(
                eventType.ToString(),
                new PostDomainEventPayload(postId, currentUserId, eventType, request.ParentId.HasValue ? "A reply was added." : "A new comment was added.")),
            cancellationToken);

        return new CommentResponse
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task DeleteCommentAsync(Guid currentUserId, Guid commentId, DeleteCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken)
            ?? throw new AppException("Comment not found.", 404);

        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken)
            ?? throw new AppException("Post not found.", 404);

        if (post.IsClosed)
        {
            throw new AppException("This post is closed and can no longer be modified.");
        }

        if (request.DeleteForAll)
        {
            var membership = await _postMemberRepository.GetByPostAndUserAsync(post.Id, currentUserId, cancellationToken);
            var isOwner = post.OwnerId == currentUserId;
            var isCommentOwner = comment.UserId == currentUserId;

            if (!isOwner && !isCommentOwner)
            {
                throw new AppException("Only the comment owner or post owner can delete for all.", 403);
            }

            comment.IsDeletedForAll = true;
            comment.Content = "[deleted]";
        }
        else
        {
            if (comment.UserId != currentUserId)
            {
                throw new AppException("You can only delete your own comment for yourself.", 403);
            }

            comment.Content = "[deleted by author]";
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventDispatcher.PublishAsync(
            new DomainEvent(
                PostEventType.CommentDeleted.ToString(),
                new PostDomainEventPayload(post.Id, currentUserId, PostEventType.CommentDeleted, "A comment was deleted.")),
            cancellationToken);
    }

    public async Task ReactAsync(Guid currentUserId, Guid commentId, ReactToCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken)
            ?? throw new AppException("Comment not found.", 404);

        var post = await _postRepository.GetByIdAsync(comment.PostId, cancellationToken)
            ?? throw new AppException("Post not found.", 404);

        if (post.IsClosed)
        {
            throw new AppException("This post is closed and can no longer be modified.");
        }

        await EnsureCanParticipateAsync(post, currentUserId, cancellationToken);

        var existingReaction = await _commentReactionRepository.GetByCommentAndUserAsync(commentId, currentUserId, cancellationToken);
        if (existingReaction is null)
        {
            await _commentReactionRepository.AddAsync(new CommentReaction
            {
                CommentId = commentId,
                UserId = currentUserId,
                Type = request.Type
            }, cancellationToken);
        }
        else
        {
            existingReaction.Type = request.Type;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventDispatcher.PublishAsync(
            new DomainEvent(
                PostEventType.CommentReacted.ToString(),
                new PostDomainEventPayload(post.Id, currentUserId, PostEventType.CommentReacted, "A comment reaction was added or updated.")),
            cancellationToken);
    }

    private async Task EnsureCanParticipateAsync(Post post, Guid currentUserId, CancellationToken cancellationToken)
    {
        if (post.Type == PostType.Public)
        {
            return;
        }

        var membership = await _postMemberRepository.GetByPostAndUserAsync(post.Id, currentUserId, cancellationToken);
        if (membership?.Status != MembershipStatus.Accepted)
        {
            throw new AppException("Private posts require accepted membership.", 403);
        }
    }
}
