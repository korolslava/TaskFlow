using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Comments.Commands;
using TaskFlow.Application.Comments.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("tasks/{taskId:guid}/comments")]
[Authorize]
public class CommentsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> GetAll(Guid taskId)
    {
        var result = await mediator.Send(new GetCommentsQuery(taskId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> Add(
        Guid taskId, AddCommentRequest req)
    {
        var result = await mediator.Send(new AddCommentCommand(
            taskId,
            CurrentUserId,
            req.BoardId,
            req.Content));
        return Ok(result);
    }
}

public record AddCommentRequest(Guid BoardId, string Content);