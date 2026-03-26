namespace TaskFlow.Domain.Entities;

public class BoardColumn
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BoardId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Order { get; private set; }

    public ICollection<TaskItem> Tasks { get; private set; } = [];

    private BoardColumn() { }

    public static BoardColumn Create(Guid boardId, string name, int order) =>
        new() { BoardId = boardId, Name = name, Order = order };
}