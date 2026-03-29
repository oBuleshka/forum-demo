using ForumBL.Core.DTOs.Comments;
using ForumBL.Core.Services;
using ForumAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/posts/{postId:guid}/comments")]
public class PostCommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public PostCommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Add(Guid postId, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var response = await _commentService.AddCommentAsync(User.GetUserId(), postId, request, cancellationToken);
        return Ok(response);
    }
}
