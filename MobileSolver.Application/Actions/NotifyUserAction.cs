using Microsoft.EntityFrameworkCore;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Actions.Users.Interfaces;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.Notifications.Email.Interfaces;

namespace MobileSolver.Application.Actions;

public class NotifyUserAction(
    IEmailSenderService emailSender,
    ITicketUsersRepository usersRepository
)
    : INotifyUserAction<MobileTickets>
{
    public async Task ExecuteAsync(MobileTickets ticket, CancellationToken cancellationToken)
    {
        var user = await usersRepository
            .GetByUserId(ticket.CreatedById)
            .Select(tu => tu.User)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            throw new UserNotFoundException();
        
        if(string.IsNullOrEmpty(user.Email))
            return;

        var subject = $"MobileSolver | Atualização sobre o chamado #{ticket.Id}";
        var message = BuildUserEmail(ticket);
        await emailSender.SendEmailAsync(user.Email, subject, message);
        
        // Poderia ser feito aqui o envio para WhatsApp, SMS, etc.
    }

    private string BuildUserEmail(Tickets ticket) => @$"
        <html>
          <body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
              <h2 style='color: #2c3e50;'>Olá,</h2>
              <p>Estamos entrando em contato para informar que houve uma <strong>atualização no seu chamado</strong> no sistema <strong>TicketSolver</strong>.</p>
              
              <p>
                <strong>ID do Chamado:</strong> #{ticket.Id}<br />
                <strong>Status atual:</strong> {ticket.Status}<br />
                <strong>Última atualização:</strong> {ticket.UpdatedAt:dd/MM/yyyy HH:mm}
              </p>
        
              <p>Para mais detalhes, acesse o sistema:</p>
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