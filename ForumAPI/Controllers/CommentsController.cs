using ForumBL.Core.DTOs.Comments;
using ForumBL.Core.Services;
using ForumAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid commentId, [FromBody] DeleteCommentRequest request, CancellationToken cancellationToken)
    {
        await _commentService.DeleteCommentAsync(User.GetUserId(), commentId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{commentId:guid}/react")]
    public async Task<IActionResult> React(Guid commentId, ReactToCommentRequest request, CancellationToken cancellationToken)
    {
        await _commentService.ReactAsync(User.GetUserId(), commentId, request, cancellationToken);
        return NoContent();
    }
}
