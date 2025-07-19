using TicketSolver.Application.Actions.Users.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Users;

public class BaseNotifyUserAction : INotifyUserAction<Tickets>
{
    public Task ExecuteAsync(Tickets ticket, CancellationToken cancellationToken)
    {
        // Não faz nada
        return Task.CompletedTask;
    }
}