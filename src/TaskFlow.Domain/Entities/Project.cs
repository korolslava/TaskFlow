namespace TaskFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Board> Boards { get; private set; } = [];
    public ICollection<Sprint> Sprints { get; private set; } = [];

    private Project() { }

    public static Project Create(Guid workspaceId, string name, string? description = null) =>
        new() { WorkspaceId = workspaceId, Name = name, Description = description };
}