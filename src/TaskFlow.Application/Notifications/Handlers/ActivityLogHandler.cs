using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Notifications.Handlers;

public class ActivityLogHandler(AppDbContext db)
    : INotificationHandler<TaskMovedEvent>,
      INotificationHandler<TaskCreatedEvent>,
      INotificationHandler<CommentAddedEvent>
{
    public async Task Handle(TaskMovedEvent notification, CancellationToken ct)
    {
        var log = ActivityLog.Create(
            Guid.Empty,
            notification.MovedByUserId,
            $"moved task to another column",
            "TaskItem",
            notification.TaskItemId);

        db.ActivityLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }

    public async Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        var log = ActivityLog.Create(
            Guid.Empty,
            notification.CreatedByUserId,
            "created a new task",
            "TaskItem",
            notification.TaskItemId);

        db.ActivityLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }

    public async Task Handle(CommentAddedEvent notification, CancellationToken ct)
    {
        var log = ActivityLog.Create(
            Guid.Empty,
            notification.AuthorId,
            "added a comment",
            "Comment",
            notification.CommentId);

        db.ActivityLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }
}