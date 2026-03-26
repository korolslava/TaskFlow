namespace TaskFlow.Domain.Entities;

public class Workspace
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public Guid OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<WorkspaceMember> Members { get; private set; } = [];
    public ICollection<Project> Projects { get; private set; } = [];

    private Workspace() { }

    public static Workspace Create(string name, Guid ownerId)
    {
        var slug = name.ToLower().Replace(" ", "-");
        return new Workspace { Name = name, Slug = slug, OwnerId = ownerId };
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (Members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User is already a member.");
        Members.Add(WorkspaceMember.Create(Id, userId, role));
    }
}