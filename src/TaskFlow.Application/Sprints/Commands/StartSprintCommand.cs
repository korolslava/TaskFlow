using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Sprints.Commands;

public record StartSprintCommand(
    Guid SprintId,
    DateTime StartDate,
    DateTime EndDate
) : IRequest;

public class StartSprintCommandValidator : AbstractValidator<StartSprintCommand>
{
    public StartSprintCommandValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date.");

        RuleFor(x => x.EndDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("End date must be in the future.");
    }
}

public class StartSprintCommandHandler(AppDbContext db)
    : IRequestHandler<StartSprintCommand>
{
    public async Task Handle(StartSprintCommand request, CancellationToken ct)
    {
        var sprint = await db.Sprints
            .FirstOrDefaultAsync(s => s.Id == request.SprintId, ct);

        if (sprint is null)
            throw new InvalidOperationException("Sprint not found.");

        sprint.Start(request.StartDate, request.EndDate);
        await db.SaveChangesAsync(ct);
    }
}