using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Sprints.Commands;

public record AssignTaskToSprintCommand(
    Guid TaskItemId,
    Guid? SprintId
) : IRequest;

public class AssignTaskToSprintCommandHandler(AppDbContext db)
    : IRequestHandler<AssignTaskToSprintCommand>
{
    public async Task Handle(
        AssignTaskToSprintCommand request, CancellationToken ct)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskItemId, ct);

        if (task is null)
            throw new InvalidOperationException("Task not found.");

        task.AssignToSprint(request.SprintId);
        await db.SaveChangesAsync(ct);
    }
}