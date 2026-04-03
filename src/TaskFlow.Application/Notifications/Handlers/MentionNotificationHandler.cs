using System.Text.RegularExpressions;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.BackgroundJobs;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.BackgroundJobs;

namespace TaskFlow.Application.Notifications.Handlers;

public partial class MentionNotificationHandler(
    AppDbContext db,
    IBackgroundJobClient jobClient)
    : INotificationHandler<CommentAddedEvent>
{
    private static readonly Regex MentionRegex = new(@"@(\w+)");

    public async Task Handle(CommentAddedEvent notification, CancellationToken ct)
    {
        var mentions = MentionRegex
            .Matches(notification.Content)
            .Select(m => m.Groups[1].Value.ToLower())
            .Distinct()
            .ToList();

        if (mentions.Count == 0) return;

        var mentionedUsers = await db.Users
            .Where(u => mentions.Contains(u.DisplayName.ToLower()))
            .ToListAsync(ct);

        var author = await db.Users
            .FirstOrDefaultAsync(u => u.Id == notification.AuthorId, ct);

        foreach (var user in mentionedUsers)
        {
            var authorName = author?.DisplayName ?? "Someone";

            jobClient.Enqueue<MentionEmailJob>(job =>
                job.SendAsync(
                    user.Email,
                    user.DisplayName,
                    authorName,
                    notification.Content,
                    notification.TaskItemId));
        }
    }
}