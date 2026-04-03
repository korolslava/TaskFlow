using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Sprints.Commands;

public record CreateSprintCommand(
    Guid ProjectId,
    Guid CreatedByUserId,
    string Name,
    string? Goal
) : IRequest<CreateSprintResult>;

public record CreateSprintResult(Guid Id, string Name, string? Goal, string Status);

public class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Goal)
            .MaximumLength(500).When(x => x.Goal is not null);
    }
}

public class CreateSprintCommandHandler(AppDbContext db)
    : IRequestHandler<CreateSprintCommand, CreateSprintResult>
{
    public async Task<CreateSprintResult> Handle(
        CreateSprintCommand request, CancellationToken ct)
    {
        var projectExists = await db.Projects
            .AnyAsync(p => p.Id == request.ProjectId, ct);

        if (!projectExists)
            throw new InvalidOperationException("Project not found.");

        var sprint = Sprint.Create(
            request.ProjectId,
            request.Name,
            request.Goal);

        db.Sprints.Add(sprint);
        await db.SaveChangesAsync(ct);

        return new CreateSprintResult(
            sprint.Id,
            sprint.Name,
            sprint.Goal,
            sprint.Status.ToString());
    }
}