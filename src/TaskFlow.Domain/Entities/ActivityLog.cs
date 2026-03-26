namespace TaskFlow.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;

    private ActivityLog() { }

    public static ActivityLog Create(Guid workspaceId, Guid userId,
        string action, string? entityType = null, Guid? entityId = null) =>
        new()
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId
        };
}