using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Comments.Commands;

public record AddCommentCommand(
    Guid TaskItemId,
    Guid AuthorId,
    Guid BoardId,
    string Content
) : IRequest<AddCommentResult>;

public record AddCommentResult(
    Guid Id,
    Guid AuthorId,
    string Content,
    DateTime CreatedAt);

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class AddCommentCommandHandler(AppDbContext db, IMediator mediator)
    : IRequestHandler<AddCommentCommand, AddCommentResult>
{
    public async Task<AddCommentResult> Handle(
        AddCommentCommand request, CancellationToken ct)
    {
        var taskExists = await db.Tasks
            .AnyAsync(t => t.Id == request.TaskItemId, ct);

        if (!taskExists)
            throw new InvalidOperationException("Task not found.");

        var comment = Comment.Create(
            request.TaskItemId,
            request.AuthorId,
            request.Content);

        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);

        await mediator.Publish(new CommentAddedEvent(
            comment.Id,
            request.TaskItemId,
            request.BoardId,
            request.AuthorId,
            request.Content), ct);

        return new AddCommentResult(
            comment.Id,
            comment.AuthorId,
            comment.Content,
            comment.CreatedAt);
    }
}