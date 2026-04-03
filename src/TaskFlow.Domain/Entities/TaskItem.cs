using System.Xml.Linq;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ColumnId { get; private set; }
    public Guid? SprintId { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public int StoryPoints { get; private set; }
    public int Order { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; private set; } = [];

    private TaskItem() { }

    public static TaskItem Create(Guid columnId, string title, int order) =>
        new() { ColumnId = columnId, Title = title, Order = order };

    public void MoveTo(Guid newColumnId, int newOrder)
    {
        ColumnId = newColumnId;
        Order = newOrder;
    }

    public void AssignTo(Guid? userId) => AssigneeId = userId;
    public void SetPriority(TaskPriority priority) => Priority = priority;
    public void SetStoryPoints(int points) => StoryPoints = points;
    public void AssignToSprint(Guid? sprintId) => SprintId = sprintId;
}