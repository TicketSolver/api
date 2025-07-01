using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace TicketSolver.Application.Actions.Ticket;

public class BaseCreateTicketAction(ITicketsRepository<Tickets> ticketsRepository) : ICreateTicketAction<Tickets>
{
    public async Task<Tickets> ExecuteAsync(Tickets ticket, TicketDTO ticketDto, CancellationToken cancellationToken)
    {
        return await ticketsRepository.AddAsync(ticket);
    }
}