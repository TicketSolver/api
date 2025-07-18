using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Users.Interfaces;

public interface INotifyUserAction<TTickets> where TTickets : Tickets
{
    Task ExecuteAsync(TTickets ticket, CancellationToken cancellationToken);
}