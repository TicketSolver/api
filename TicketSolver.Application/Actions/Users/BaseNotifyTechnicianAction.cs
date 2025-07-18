using TicketSolver.Application.Actions.Users.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Users;

public class BaseNotifyTechnicianAction : INotifyTechcnicianAction<Tickets>
{
    public Task ExecuteAsync(Tickets ticket, CancellationToken cancellationToken)
    {
        return Task.CompletedTask; // Não faz nada.
    }
}