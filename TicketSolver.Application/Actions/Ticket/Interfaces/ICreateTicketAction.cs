using TicketSolver.Application.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Actions.Ticket.Interfaces;

public interface ICreateTicketAction<TTickets> where TTickets : Tickets
{
    Task<TTickets> ExecuteAsync(Tickets ticket, TicketDTO ticketDto, CancellationToken cancellationToken);
}