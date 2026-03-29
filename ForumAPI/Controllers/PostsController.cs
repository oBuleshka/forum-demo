using ForumBL.Core.DTOs.Posts;
using ForumBL.Core.Services;
using ForumAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumAPI.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostResponse>> Create(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var response = await _postService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("invited")]
    public async Task<ActionResult<IReadOnlyList<InvitedPostResponse>>> GetInvited(CancellationToken cancellationToken)
    {
        var response = await _postService.GetInvitedPostsAsync(User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<PostResponse>> GetById(Guid postId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : Guid.Empty;
        var response = await _postService.GetByIdAsync(userId, postId, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("{postId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid postId, InviteUserRequest request, CancellationToken cancellationToken)
    {
        await _postService.InviteUserToPostAsync(User.GetUserId(), postId, request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{postId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid postId, CancellationToken cancellationToken)
    {
        await _postService.AcceptInvitationAsync(User.GetUserId(), postId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{postId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid postId, CancellationToken cancellationToken)
    {
        await _postService.RejectInvitationAsync(User.GetUserId(), postId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{postId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid postId, Guid userId, CancellationToken cancellationToken)
    {
        await _postService.RemoveMemberAsync(User.GetUserId(), postId, userId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{postId:guid}/close")]
    public async Task<IActionResult> Close(Guid postId, CancellationToken cancellationToken)
    {
        await _postService.ClosePostAsync(User.GetUserId(), postId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{postId:guid}/stats")]
    public async Task<ActionResult<PostStatsResponse>> GetStats(Guid postId, CancellationToken cancellationToken)
    {
        var response = await _postService.GetStatsAsync(User.GetUserId(), postId, cancellationToken);
        return Ok(response);
    }
}
