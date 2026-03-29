using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Projects.Commands;

public record CreateProjectCommand(
    Guid WorkspaceId,
    Guid CreatedByUserId,
    string Name,
    string? Description
) : IRequest<CreateProjectResult>;

public record CreateProjectResult(Guid Id, string Name, string? Description);

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description is not null);
    }
}

public class CreateProjectCommandHandler(AppDbContext db)
    : IRequestHandler<CreateProjectCommand, CreateProjectResult>
{
    public async Task<CreateProjectResult> Handle(
        CreateProjectCommand request, CancellationToken ct)
    {
        var workspaceExists = await db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId, ct);

        if (!workspaceExists)
            throw new InvalidOperationException("Workspace not found.");

        var project = Project.Create(
            request.WorkspaceId,
            request.Name,
            request.Description);

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);

        return new CreateProjectResult(
            project.Id,
            project.Name,
            project.Description);
    }
}