namespace TaskFlow.Domain.Entities;

public class WorkspaceMember
{
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public User? User { get; private set; }
    public Workspace? Workspace { get; private set; }

    private WorkspaceMember() { }

    public static WorkspaceMember Create(Guid workspaceId, Guid userId, WorkspaceRole role) =>
        new()
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };
}