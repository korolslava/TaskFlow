using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Workspaces.Queries;

public record GetWorkspaceQuery(Guid WorkspaceId, Guid RequestedByUserId)
    : IRequest<GetWorkspaceResult?>;

public record GetWorkspaceResult(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAt,
    List<WorkspaceMemberDto> Members
);

public record WorkspaceMemberDto(Guid UserId, string DisplayName, string Role);

public class GetWorkspaceQueryHandler(AppDbContext db)
    : IRequestHandler<GetWorkspaceQuery, GetWorkspaceResult?>
{
    public async Task<GetWorkspaceResult?> Handle(
        GetWorkspaceQuery request, CancellationToken ct)
    {
        var workspace = await db.Workspaces
            .Include(w => w.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, ct);

        if (workspace is null) return null;

        var isMember = workspace.Members
            .Any(m => m.UserId == request.RequestedByUserId);

        if (!isMember) return null;

        return new GetWorkspaceResult(
            workspace.Id,
            workspace.Name,
            workspace.Slug,
            workspace.CreatedAt,
            workspace.Members.Select(m => new WorkspaceMemberDto(
                m.UserId,
                m.User!.DisplayName,
                m.Role.ToString()
            )).ToList()
        );
    }
}