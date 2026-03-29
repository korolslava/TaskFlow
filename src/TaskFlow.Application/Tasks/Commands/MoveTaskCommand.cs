using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Tasks.Commands;

public record MoveTaskCommand(
    Guid TaskItemId,
    Guid ToColumnId,
    int NewOrder,
    Guid BoardId,
    Guid MovedByUserId
) : IRequest;

public class MoveTaskCommandHandler(AppDbContext db, IMediator mediator)
    : IRequestHandler<MoveTaskCommand>
{
    public async Task Handle(
        MoveTaskCommand request, CancellationToken ct)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskItemId, ct);

        if (task is null)
            throw new InvalidOperationException("Task not found.");

        var fromColumnId = task.ColumnId;

        task.MoveTo(request.ToColumnId, request.NewOrder);
        await db.SaveChangesAsync(ct);

        await mediator.Publish(new Domain.Events.TaskMovedEvent(
            task.Id,
            request.BoardId,
            fromColumnId,
            request.ToColumnId,
            request.MovedByUserId), ct);
    }
}