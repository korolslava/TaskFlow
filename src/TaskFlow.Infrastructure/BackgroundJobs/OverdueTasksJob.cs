using Dapper;
using MailKit.Net.Smtp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace TaskFlow.Infrastructure.BackgroundJobs;

public class OverdueTasksJob(
    IConfiguration config,
    ILogger<OverdueTasksJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Checking overdue tasks...");

        const string sql = """
            SELECT t.Id, t.Title, t.DueDate,
                   u.Email AS AssigneeEmail,
                   u.DisplayName AS AssigneeName
            FROM Tasks t
            JOIN Users u ON u.Id = t.AssigneeId
            JOIN Columns c ON c.Id = t.ColumnId
            WHERE t.DueDate < GETUTCDATE()
              AND c.Name != 'Done'
              AND t.AssigneeId IS NOT NULL
            """;

        await using var connection = new SqlConnection(
            config.GetConnectionString("DefaultConnection"));

        var overdueTasks = await connection.QueryAsync<dynamic>(sql);

        foreach (var task in overdueTasks)
        {
            await SendOverdueEmailAsync(
                (string)task.AssigneeEmail,
                (string)task.AssigneeName,
                (string)task.Title,
                (DateTime)task.DueDate);
        }

        logger.LogInformation("Overdue tasks check completed.");
    }

    private async Task SendOverdueEmailAsync(
        string email, string name, string taskTitle, DateTime dueDate)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(
                config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = $"Overdue task: {taskTitle}";
            message.Body = new TextPart("plain")
            {
                Text = $"Hi {name},\n\n" +
                       $"Task '{taskTitle}' was due on " +
                       $"{dueDate:dd MMM yyyy} and is still not completed.\n\n" +
                       $"Please update the task status.\n\nTaskFlow"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                config["Email:Host"],
                int.Parse(config["Email:Port"]!),
                false);
            await smtp.AuthenticateAsync(
                config["Email:Username"],
                config["Email:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send overdue email to {Email}", email);
        }
    }
}