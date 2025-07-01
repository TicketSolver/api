using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Ticket.Interfaces;

public interface ICreateTicketAction
{
    Task<Tickets> ExecuteAsync(Tickets ticket, CancellationToken cancellationToken);
}