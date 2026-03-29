using ForumBL.Core.Abstractions.Events;
using ForumBL.Core.Abstractions.Repositories;
using ForumBL.Core.DTOs.Comments;
using ForumBL.Core.Exceptions;
using ForumBL.Core.Services;
using Forum.Domain.Entities;
using Forum.Domain.Enums;
using Moq;

namespace ForumBL.Core.Tests.Services;

public class CommentServiceTests
{
    [Fact]
    public async Task AddCommentAsync_ShouldThrow_WhenPostIsClosed()
    {
        var commentRepository = new Mock<ICommentRepository>();
        var reactionRepository = new Mock<ICommentReactionRepository>();
        var postRepository = new Mock<IPostRepository>();
        var postMemberRepository = new Mock<IPostMemberRepository>();
        var unitOfWork = new Mock<IForumUnitOfWork>();
        var dispatcher = new Mock<IEventDispatcher>();

        postRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Post
            {
                Id = Guid.NewGuid(),
                Type = PostType.Public,
                IsClosed = true
            });

        var service = new CommentService(
            commentRepository.Object,
            reactionRepository.Object,
            postRepository.Object,
            postMemberRepository.Object,
            unitOfWork.Object,
            dispatcher.Object);

        var act = () => service.AddCommentAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateCommentRequest
        {
            Content = "hello"
        });

        var exception = await Assert.ThrowsAsync<AppException>(act);
        Assert.Equal("Comments are closed for this post.", exception.Message);
    }
}
