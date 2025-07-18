using Microsoft.EntityFrameworkCore;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Actions.Users.Interfaces;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.Notifications.Email.Interfaces;

namespace MobileSolver.Application.Actions;

public class NotifyTechnicianAction(
    IEmailSenderService emailSender,
    ITicketUsersRepository ticketUsersRepository
) : INotifyTechcnicianAction<MobileTickets>
{
    public async Task ExecuteAsync(MobileTickets ticket, CancellationToken cancellationToken)
    {
        var users = await ticketUsersRepository
            .GetAll()
            .Where(t => t.TicketId == ticket.Id)
            .Select(tu => tu.User)
            .ToListAsync(cancellationToken);

        var subject = $"MobileSolver | Atualização sobre o chamado #{ticket.Id}";

        foreach (var user in users) // Notifica todos os responsáveis
        {
            if (string.IsNullOrEmpty(user.Email))
                continue;

            var message = BuildTechnicianEmail(ticket, user.FullName);
            await emailSender.SendEmailAsync(user.Email, subject, message);
            // Poderia ser feito aqui o envio para WhatsApp, SMS, etc.
        }
    }

    private string BuildTechnicianEmail(MobileTickets ticket, string technicianName) => $@"
        <html>
          <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
              <h2 style='color: #2c3e50;'>Olá, {technicianName}!</h2>
              <p>Você foi <strong>atribuído</strong> ou houve uma <strong>atualização</strong> em um chamado no sistema <strong>TicketSolver</strong>.</p>

              <p>
                <strong>ID do Chamado:</strong> #{ticket.Id}<br />
                <strong>Título:</strong> {ticket.Title}<br />
                <strong>Modelo/Versão:</strong> {ticket.DeviceModel}/{ticket.Version}<br />
                <strong>Status:</strong> {ticket.Status}<br />
                <strong>Solicitante:</strong> {ticket.CreatedBy.FullName}<br />
                <strong>Data de Criação:</strong> {ticket.CreatedAt:dd/MM/yyyy HH:mm}
              </p>

              <p>Para acessar os detalhes completos, clique no botão abaixo:</p>
              <p>
                <a href='https://app.ticketsolver.com/tickets/{ticket.Id}' 
                   style='display: inline-block; padding: 10px 20px; background-color: #0066cc; color: white; text-decoration: none; border-radius: 5px;'>
                  Ver Chamado
                </a>
              </p>

              <p>Atenciosamente,<br />Equipe TicketSolver</p>
              <hr style='margin-top: 40px;' />
              <small style='color: #999;'>Esta é uma mensagem automática. Não responda este e-mail.</small>
            </div>
          </body>
        </html>
    ";
}