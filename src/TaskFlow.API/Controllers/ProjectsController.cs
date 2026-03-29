using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Projects.Commands;
using TaskFlow.Application.Projects.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("workspaces/{workspaceId:guid}/projects")]
[Authorize]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> GetAll(Guid workspaceId)
    {
        var result = await mediator.Send(
            new GetProjectsQuery(workspaceId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "WorkspaceAdmin")]
    public async Task<IActionResult> Create(
        Guid workspaceId, CreateProjectRequest req)
    {
        var result = await mediator.Send(
            new CreateProjectCommand(
                workspaceId,
                CurrentUserId,
                req.Name,
                req.Description));

        return Ok(result);
    }
}

public record CreateProjectRequest(string Name, string? Description);