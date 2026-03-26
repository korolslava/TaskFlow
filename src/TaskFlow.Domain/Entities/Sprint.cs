namespace TaskFlow.Domain.Entities;

public class Sprint
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Goal { get; private set; }
    public SprintStatus Status { get; private set; } = SprintStatus.Planning;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public ICollection<TaskItem> Tasks { get; private set; } = [];

    private Sprint() { }

    public static Sprint Create(Guid projectId, string name, string? goal = null) =>
        new() { ProjectId = projectId, Name = name, Goal = goal };

    public void Start(DateTime startDate, DateTime endDate)
    {
        if (Status != SprintStatus.Planning)
            throw new InvalidOperationException("Only planning sprints can be started.");
        Status = SprintStatus.Active;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Complete() => Status = SprintStatus.Completed;
}