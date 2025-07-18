namespace TicketSolver.Infra.Notifications.Email.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
