using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Boards.Commands;

public record CreateBoardCommand(
    Guid ProjectId,
    Guid CreatedByUserId,
    string Name
) : IRequest<CreateBoardResult>;

public record CreateBoardResult(Guid Id, string Name, Guid ProjectId);

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public class CreateBoardCommandHandler(AppDbContext db)
    : IRequestHandler<CreateBoardCommand, CreateBoardResult>
{
    public async Task<CreateBoardResult> Handle(
        CreateBoardCommand request, CancellationToken ct)
    {
        var projectExists = await db.Projects
            .AnyAsync(p => p.Id == request.ProjectId, ct);

        if (!projectExists)
            throw new InvalidOperationException("Project not found.");

        var board = Board.Create(request.ProjectId, request.Name);

        db.Boards.Add(board);
        await db.SaveChangesAsync(ct);

        return new CreateBoardResult(board.Id, board.Name, board.ProjectId);
    }
}