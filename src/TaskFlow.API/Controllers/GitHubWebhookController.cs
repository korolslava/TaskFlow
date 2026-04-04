using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Tasks.Commands;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("webhooks/github")]
public class GitHubWebhookController(
    IMediator mediator,
    AppDbContext db,
    IConfiguration config) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (!VerifySignature(body))
            return Unauthorized("Invalid signature.");

        var payload = JsonSerializer.Deserialize<JsonElement>(body);

        var action = payload.GetProperty("action").GetString();
        var merged = payload
            .GetProperty("pull_request")
            .GetProperty("merged")
            .GetBoolean();

        if (action != "closed" || !merged)
            return Ok("Ignored.");

        var prTitle = payload
            .GetProperty("pull_request")
            .GetProperty("title")
            .GetString() ?? "";

        var prBody = payload
            .GetProperty("pull_request")
            .GetProperty("body")
            .GetString() ?? "";

        var taskIds = ParseTaskIds($"{prTitle} {prBody}");

        foreach (var taskId in taskIds)
        {
            await MoveTaskToDoneAsync(taskId);
        }

        return Ok($"Processed {taskIds.Count} tasks.");
    }

    private bool VerifySignature(string body)
    {
        var secret = config["GitHub:WebhookSecret"];
        if (string.IsNullOrEmpty(secret)) return false;

        var signature = Request.Headers["X-Hub-Signature-256"]
            .ToString();

        if (string.IsNullOrEmpty(signature)) return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var expectedSignature = $"sha256={Convert.ToHexString(hash).ToLower()}";

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature));
    }

    private static List<Guid> ParseTaskIds(string text)
    {
        var regex = new Regex(@"#TASK-([0-9a-fA-F-]{36})");
        return regex.Matches(text)
            .Select(m => Guid.TryParse(m.Groups[1].Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
    }

    private async Task MoveTaskToDoneAsync(Guid taskId)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null) return;

        var doneColumn = await db.Columns
            .Include(c => c.Tasks)
            .Where(c => c.Name == "Done")
            .FirstOrDefaultAsync(c => db.Columns
                .Any(other => other.BoardId == c.BoardId
                    && other.Id == task.ColumnId));

        if (doneColumn is null) return;

        var maxOrder = doneColumn.Tasks.Any()
            ? doneColumn.Tasks.Max(t => t.Order)
            : -1;

        var column = await db.Columns
            .FirstOrDefaultAsync(c => c.Id == task.ColumnId);

        if (column is null) return;

        await mediator.Send(new MoveTaskCommand(
            taskId,
            doneColumn.Id,
            maxOrder + 1,
            column.BoardId,
            Guid.Empty));
    }
}