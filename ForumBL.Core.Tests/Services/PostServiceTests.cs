using ForumBL.Core.Abstractions.Events;
using ForumBL.Core.Abstractions.Repositories;
using ForumBL.Core.DTOs.Posts;
using ForumBL.Core.Exceptions;
using ForumBL.Core.Services;
using Forum.Domain.Entities;
using Forum.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace ForumBL.Core.Tests.Services;

public class PostServiceTests
{
    [Fact]
    public async Task AcceptInvitationAsync_ShouldThrow_WhenCapacityIsReached()
    {
        var postRepository = new Mock<IPostRepository>();
        var postMemberRepository = new Mock<IPostMemberRepository>();
        var commentRepository = new Mock<ICommentRepository>();
        var commentReactionRepository = new Mock<ICommentReactionRepository>();
        var postEventRepository = new Mock<IPostEventRepository>();
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IForumUnitOfWork>();
        var dispatcher = new Mock<IEventDispatcher>();

        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        postRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Post
            {
                Id = postId,
                Capacity = 1,
                IsClosed = false,
                Type = PostType.Private
            });

        postMemberRepository.Setup(x => x.GetByPostAndUserAsync(postId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostMember
            {
                PostId = postId,
                UserId = userId,
                Status = MembershipStatus.Pending,
                Role = PostRole.Member
            });

        postMemberRepository.Setup(x => x.CountByStatusAsync(postId, MembershipStatus.Accepted, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService(
            postRepository,
            postMemberRepository,
            commentRepository,
            commentReactionRepository,
            postEventRepository,
            userRepository,
            unitOfWork,
            dispatcher);

        var act = () => service.AcceptInvitationAsync(userId, postId);

        var exception = await Assert.ThrowsAsync<AppException>(act);
        Assert.Equal("Post capacity has been reached.", exception.Message);
    }

    [Fact]
    public async Task RemoveMemberAsync_ShouldThrow_WhenCallerIsNotOwner()
    {
        var postRepository = new Mock<IPostRepository>();
        var postMemberRepository = new Mock<IPostMemberRepository>();
        var commentRepository = new Mock<ICommentRepository>();
        var commentReactionRepository = new Mock<ICommentReactionRepository>();
        var postEventRepository = new Mock<IPostEventRepository>();
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IForumUnitOfWork>();
        var dispatcher = new Mock<IEventDispatcher>();

        var callerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        postRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Post
            {
                Id = postId,
                OwnerId = Guid.NewGuid(),
                IsClosed = false
            });

        var service = CreateService(
            postRepository,
            postMemberRepository,
            commentRepository,
            commentReactionRepository,
            postEventRepository,
            userRepository,
            unitOfWork,
            dispatcher);

        var act = () => service.RemoveMemberAsync(callerId, postId, memberId);

        var exception = await Assert.ThrowsAsync<AppException>(act);
        Assert.Equal("Only the post owner can perform this action.", exception.Message);
    }

    private static PostService CreateService(
        Mock<IPostRepository> postRepository,
        Mock<IPostMemberRepository> postMemberRepository,
        Mock<ICommentRepository> commentRepository,
        Mock<ICommentReactionRepository> commentReactionRepository,
        Mock<IPostEventRepository> postEventRepository,
        Mock<IUserRepository> userRepository,
        Mock<IForumUnitOfWork> unitOfWork,
        Mock<IEventDispatcher> dispatcher)
    {
        return new PostService(
            postRepository.Object,
            postMemberRepository.Object,
            commentRepository.Object,
            commentReactionRepository.Object,
            postEventRepository.Object,
            userRepository.Object,
            unitOfWork.Object,
            dispatcher.Object,
            new MemoryCache(new MemoryCacheOptions()));
    }
}
