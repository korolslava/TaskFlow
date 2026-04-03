using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace TaskFlow.Infrastructure.BackgroundJobs;

public class MentionEmailJob(
    IConfiguration config,
    ILogger<MentionEmailJob> logger)
{
    public async Task SendAsync(
        string toEmail,
        string toName,
        string authorName,
        string commentContent,
        Guid taskItemId)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = $"{authorName} mentioned you in a comment";
            message.Body = new TextPart("plain")
            {
                Text = $"Hi {toName},\n\n" +
                       $"{authorName} mentioned you in a comment:\n\n" +
                       $"\"{commentContent}\"\n\n" +
                       $"Task ID: {taskItemId}\n\nTaskFlow"
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
                "Failed to send mention email to {Email}", toEmail);
        }
    }
}