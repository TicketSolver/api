using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace MobileSolver.Api.Application.Actions;

public class CreateTicketAction(ITicketsRepository ticketsRepository) : ICreateTicketAction
{
    public async Task<Tickets> ExecuteAsync(Tickets ticket, CancellationToken cancellationToken)
    {
        // Faz algum cast e adiciona/modifica propriedades   
        return await ticketsRepository.AddAsync(ticket);
    }
}