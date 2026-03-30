using MediatR;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.RealTime;

namespace TaskFlow.Application.Notifications.Handlers;

public class TaskCreatedSignalRHandler(IHubContext<BoardHub> hubContext)
    : INotificationHandler<TaskCreatedEvent>
{
    public async Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        await hubContext.Clients
            .Group($"board:{notification.BoardId}")
            .SendAsync("TaskCreated", new
            {
                taskItemId = notification.TaskItemId,
                columnId = notification.ColumnId,
                createdBy = notification.CreatedByUserId,
                occurredAt = notification.OccurredAt
            }, ct);
    }
}