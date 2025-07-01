using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace TicketSolver.Application.Actions.Ticket;

public class BaseCreateTicketAction(ITicketsRepository ticketsRepository) : ICreateTicketAction
{
    public async Task<Tickets> ExecuteAsync(Tickets ticket, CancellationToken cancellationToken)
    {
        return await ticketsRepository.AddAsync(ticket);
    }
}