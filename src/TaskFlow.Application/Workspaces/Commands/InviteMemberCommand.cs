using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Application.Workspaces.Commands;

public record InviteMemberCommand(
    Guid WorkspaceId,
    Guid InvitedByUserId,
    string Email,
    WorkspaceRole Role
) : IRequest<InviteMemberResult>;

public record InviteMemberResult(Guid UserId, string DisplayName, string Role);

public class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.");

        RuleFor(x => x.Role)
            .NotEqual(WorkspaceRole.Owner)
            .WithMessage("Cannot invite another owner.");
    }
}

public class InviteMemberCommandHandler(AppDbContext db)
    : IRequestHandler<InviteMemberCommand, InviteMemberResult>
{
    public async Task<InviteMemberResult> Handle(
        InviteMemberCommand request, CancellationToken ct)
    {
        var inviter = await db.Members
            .FirstOrDefaultAsync(m =>
                m.WorkspaceId == request.WorkspaceId &&
                m.UserId == request.InvitedByUserId, ct);

        if (inviter is null || inviter.Role < WorkspaceRole.Admin)
            throw new UnauthorizedAccessException(
                "Only admins and owners can invite members.");

        var userToInvite = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (userToInvite is null)
            throw new InvalidOperationException("User with this email not found.");

        var alreadyMember = await db.Members
            .AnyAsync(m =>
                m.WorkspaceId == request.WorkspaceId &&
                m.UserId == userToInvite.Id, ct);

        if (alreadyMember)
            throw new InvalidOperationException("User is already a member.");

        var workspace = await db.Workspaces
            .Include(w => w.Members)
            .FirstAsync(w => w.Id == request.WorkspaceId, ct);

        workspace.AddMember(userToInvite.Id, request.Role);
        await db.SaveChangesAsync(ct);

        return new InviteMemberResult(
            userToInvite.Id,
            userToInvite.DisplayName,
            request.Role.ToString());
    }
}