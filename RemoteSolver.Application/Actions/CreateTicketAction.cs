using RemoteSolver.Application.Models.Tickets;
using RemoteSolver.Domain.Persistence.Entities;
using TicketSolver.Application.Actions.Ticket.Interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;

namespace RemoteSolver.Application.Actions;

public class CreateTicketAction(ITicketsRepository<RemoteTickets> ticketsRepository)
    : ICreateTicketAction<RemoteTickets>
{
    public async Task<RemoteTickets> ExecuteAsync(Tickets ticket, TicketDTO ticketDto,
        CancellationToken cancellationToken)
    {
        var remoteTicket = ticket as RemoteTickets;
        var remoteTicketDto = ticketDto as RemoteTicketDto;

        remoteTicket.RemoteAccessLink = remoteTicketDto.RemoteAccessLink;
        remoteTicket.AccessCredentials = remoteTicketDto.AccessCredentials;
        remoteTicket.OperatingSystem = remoteTicketDto.OperatingSystem;
        
        return await ticketsRepository.AddAsync(remoteTicket);
    }
}
