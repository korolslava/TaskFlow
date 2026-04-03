using MediatR;
using Microsoft.AspNetCore.SignalR;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.RealTime;

namespace TaskFlow.Application.Notifications.Handlers;

public class CommentAddedSignalRHandler(IHubContext<BoardHub> hubContext)
    : INotificationHandler<CommentAddedEvent>
{
    public async Task Handle(CommentAddedEvent notification, CancellationToken ct)
    {
        await hubContext.Clients
            .Group($"board:{notification.BoardId}")
            .SendAsync("CommentAdded", new
            {
                commentId = notification.CommentId,
                taskItemId = notification.TaskItemId,
                authorId = notification.AuthorId,
                content = notification.Content,
                occurredAt = notification.OccurredAt
            }, ct);
    }
}