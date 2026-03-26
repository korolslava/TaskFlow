namespace TaskFlow.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<BoardColumn> Columns { get; private set; } = [];

    private Board() { }

    public static Board Create(Guid projectId, string name)
    {
        var board = new Board { ProjectId = projectId, Name = name };
        board.Columns.Add(BoardColumn.Create(board.Id, "Backlog", 0));
        board.Columns.Add(BoardColumn.Create(board.Id, "To Do", 1));
        board.Columns.Add(BoardColumn.Create(board.Id, "In Progress", 2));
        board.Columns.Add(BoardColumn.Create(board.Id, "Done", 3));
        return board;
    }
}