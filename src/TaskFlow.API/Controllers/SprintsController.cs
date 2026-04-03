using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Sprints.Commands;
using TaskFlow.Application.Sprints.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("projects/{projectId:guid}/sprints")]
[Authorize]
public class SprintsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Policy = "WorkspaceAdmin")]
    public async Task<IActionResult> Create(
        Guid projectId, CreateSprintRequest req)
    {
        var result = await mediator.Send(new CreateSprintCommand(
            projectId, CurrentUserId, req.Name, req.Goal));
        return Ok(result);
    }

    [HttpPost("{sprintId:guid}/start")]
    [Authorize(Policy = "WorkspaceAdmin")]
    public async Task<IActionResult> Start(
        Guid sprintId, StartSprintRequest req)
    {
        await mediator.Send(new StartSprintCommand(
            sprintId, req.StartDate, req.EndDate));
        return NoContent();
    }

    [HttpPost("{sprintId:guid}/tasks/{taskId:guid}")]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> AssignTask(
        Guid sprintId, Guid taskId)
    {
        await mediator.Send(new AssignTaskToSprintCommand(taskId, sprintId));
        return NoContent();
    }

    [HttpGet("{sprintId:guid}/burndown")]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> GetBurnDown(Guid sprintId)
    {
        var result = await mediator.Send(new GetBurnDownQuery(sprintId));
        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record CreateSprintRequest(string Name, string? Goal);
public record StartSprintRequest(DateTime StartDate, DateTime EndDate);