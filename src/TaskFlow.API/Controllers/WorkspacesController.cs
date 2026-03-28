using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Workspaces.Commands;
using TaskFlow.Application.Workspaces.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("workspaces")]
[Authorize]
public class WorkspacesController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkspaceRequest req)
    {
        var result = await mediator.Send(
            new CreateWorkspaceCommand(req.Name, CurrentUserId));

        return CreatedAtAction(nameof(GetById),
            new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(
            new GetWorkspaceQuery(id, CurrentUserId));

        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record CreateWorkspaceRequest(string Name);