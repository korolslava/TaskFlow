using MediatR;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using FluentValidation;

namespace TaskFlow.Application.Workspaces.Commands;

public record CreateWorkspaceCommand(
    string Name,
    Guid OwnerId
) : IRequest<CreateWorkspaceResult>;

public record CreateWorkspaceResult(Guid Id, string Name, string Slug);

public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
    }
}

public class CreateWorkspaceCommandHandler(AppDbContext db)
    : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResult>
{
    public async Task<CreateWorkspaceResult> Handle(
        CreateWorkspaceCommand request, CancellationToken ct)
    {
        var workspace = Workspace.Create(request.Name, request.OwnerId);

        workspace.AddMember(request.OwnerId, WorkspaceRole.Owner);

        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(ct);

        return new CreateWorkspaceResult(workspace.Id, workspace.Name, workspace.Slug);
    }
}