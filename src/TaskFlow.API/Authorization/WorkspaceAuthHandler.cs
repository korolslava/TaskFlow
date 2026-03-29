using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Authorization;

public class WorkspaceAuthHandler(AppDbContext db)
    : AuthorizationHandler<WorkspaceRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceRequirement requirement)
    {
        var userIdClaim = context.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim is null) return;
        var userId = Guid.Parse(userIdClaim);

        var httpContext = context.Resource as HttpContext;
        var workspaceIdStr = httpContext?.Request.RouteValues["id"]?.ToString();

        if (workspaceIdStr is null || !Guid.TryParse(workspaceIdStr, out var workspaceId))
            return;

        var member = await db.Members
            .FirstOrDefaultAsync(m =>
                m.WorkspaceId == workspaceId &&
                m.UserId == userId);

        if (member is null) return;

        if (member.Role >= requirement.MinimumRole)
            context.Succeed(requirement);
    }
}