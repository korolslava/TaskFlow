using MediatR;

namespace TaskFlow.Domain.Events;

public abstract record DomainEvent : INotification
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record TaskMovedEvent(
    Guid TaskItemId,
    Guid BoardId,
    Guid FromColumnId,
    Guid ToColumnId,
    Guid MovedByUserId
) : DomainEvent;

public record TaskCreatedEvent(
    Guid TaskItemId,
    Guid BoardId,
    Guid ColumnId,
    Guid CreatedByUserId
) : DomainEvent;

public record CommentAddedEvent(
    Guid CommentId,
    Guid TaskItemId,
    Guid BoardId,
    Guid AuthorId,
    string Content
) : DomainEvent;