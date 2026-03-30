using MediatR;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.RealTime;

namespace TaskFlow.Application.Notifications.Handlers;

public class TaskMovedSignalRHandler(IHubContext<BoardHub> hubContext)
    : INotificationHandler<TaskMovedEvent>
{
    public async Task Handle(TaskMovedEvent notification, CancellationToken ct)
    {
        await hubContext.Clients
            .Group($"board:{notification.BoardId}")
            .SendAsync("TaskMoved", new
            {
                taskItemId = notification.TaskItemId,
                fromColumnId = notification.FromColumnId,
                toColumnId = notification.ToColumnId,
                movedBy = notification.MovedByUserId,
                occurredAt = notification.OccurredAt
            }, ct);
    }
}