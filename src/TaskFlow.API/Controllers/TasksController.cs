using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Tasks.Commands;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("boards/{boardId:guid}/tasks")]
[Authorize]
public class TasksController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> Create(
        Guid boardId, CreateTaskRequest req)
    {
        var result = await mediator.Send(new CreateTaskCommand(
            req.ColumnId,
            CurrentUserId,
            boardId,
            req.Title,
            req.Description,
            req.Priority,
            req.StoryPoints));
        return Ok(result);
    }

    [HttpPatch("{taskId:guid}/move")]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> Move(
        Guid boardId, Guid taskId, MoveTaskRequest req)
    {
        await mediator.Send(new MoveTaskCommand(
            taskId,
            req.ToColumnId,
            req.NewOrder,
            boardId,
            CurrentUserId));
        return NoContent();
    }
}

public record CreateTaskRequest(
    Guid ColumnId,
    string Title,
    string? Description,
    TaskPriority Priority,
    int StoryPoints);

public record MoveTaskRequest(Guid ToColumnId, int NewOrder);