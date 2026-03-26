namespace TaskFlow.Domain.Entities;

public class Comment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TaskItemId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Comment() { }

    public static Comment Create(Guid taskItemId, Guid authorId, string content) =>
        new() { TaskItemId = taskItemId, AuthorId = authorId, Content = content };
}