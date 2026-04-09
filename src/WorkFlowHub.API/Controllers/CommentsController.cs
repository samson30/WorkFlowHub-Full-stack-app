using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlowHub.Core.DTOs.Tasks;
using WorkFlowHub.Core.Interfaces.Services;

namespace WorkFlowHub.API.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
[Authorize]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(Guid taskId)
    {
        var comments = await _commentService.GetCommentsAsync(taskId, CurrentUserId);
        return Ok(comments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddComment(Guid taskId, [FromBody] CreateCommentDto dto)
    {
        var comment = await _commentService.AddCommentAsync(taskId, dto, CurrentUserId);
        return CreatedAtAction(nameof(GetComments), new { taskId }, comment);
    }

    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid commentId)
    {
        await _commentService.DeleteCommentAsync(commentId, CurrentUserId);
        return NoContent();
    }
}
