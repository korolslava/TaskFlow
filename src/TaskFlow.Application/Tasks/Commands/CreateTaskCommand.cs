using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Tasks.Commands;

public record CreateTaskCommand(
    Guid ColumnId,
    Guid CreatedByUserId,
    Guid BoardId,
    string Title,
    string? Description,
    TaskPriority Priority,
    int StoryPoints
) : IRequest<CreateTaskResult>;

public record CreateTaskResult(Guid Id, string Title, int Order);

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.StoryPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Story points cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Story points cannot exceed 100.");
    }
}

public class CreateTaskCommandHandler(AppDbContext db, IMediator mediator)
    : IRequestHandler<CreateTaskCommand, CreateTaskResult>
{
    public async Task<CreateTaskResult> Handle(
        CreateTaskCommand request, CancellationToken ct)
    {
        var columnExists = await db.Columns
            .AnyAsync(c => c.Id == request.ColumnId, ct);

        if (!columnExists)
            throw new InvalidOperationException("Column not found.");

        var maxOrder = await db.Tasks
            .Where(t => t.ColumnId == request.ColumnId)
            .Select(t => (int?)t.Order)
            .MaxAsync(ct) ?? -1;

        var task = TaskItem.Create(
            request.ColumnId,
            request.Title,
            maxOrder + 1);

        task.SetPriority(request.Priority);
        task.SetStoryPoints(request.StoryPoints);

        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);

        await mediator.Publish(new Domain.Events.TaskCreatedEvent(
            task.Id,
            request.BoardId,
            request.ColumnId,
            request.CreatedByUserId), ct);

        return new CreateTaskResult(task.Id, task.Title, task.Order);
    }
}