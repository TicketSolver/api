using TicketSolver.Infra.Notifications.Email.Interfaces;
using TicketSolver.Infra.Notifications.Email.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TicketSolver.Infra.Notifications.Email;

public class EmailSenderService(EmailSettings settings) : IEmailSenderService
{
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.SmtpServer, settings.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings.Username, settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}