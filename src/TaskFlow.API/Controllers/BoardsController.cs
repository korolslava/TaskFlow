using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.Boards.Commands;
using TaskFlow.Application.Boards.Queries;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("projects/{projectId:guid}/boards")]
[Authorize]
public class BoardsController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Policy = "WorkspaceAdmin")]
    public async Task<IActionResult> Create(
        Guid projectId, CreateBoardRequest req)
    {
        var result = await mediator.Send(
            new CreateBoardCommand(projectId, CurrentUserId, req.Name));
        return Ok(result);
    }

    [HttpGet("{boardId:guid}")]
    [Authorize(Policy = "WorkspaceMember")]
    public async Task<IActionResult> GetBoard(Guid boardId)
    {
        var result = await mediator.Send(new GetBoardQuery(boardId));
        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record CreateBoardRequest(string Name);