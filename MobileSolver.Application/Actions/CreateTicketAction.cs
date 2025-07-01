using MobileSolver.Application.Models.Tickets;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace MobileSolver.Application.Actions;

public class CreateTicketAction(ITicketsRepository<MobileTickets> ticketsRepository)
    : ICreateTicketAction<MobileTickets>
{
    public async Task<MobileTickets> ExecuteAsync(Tickets ticket, TicketDTO ticketDto,
        CancellationToken cancellationToken)
    {
        var mobileTicket = ticket as MobileTickets;
        var mobileTicketDto = ticketDto as MobileTicketDto;
        
        return await ticketsRepository.AddAsync(mobileTicket);
    }
}