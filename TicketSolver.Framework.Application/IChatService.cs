using TicketSolver.Application.Models;

namespace TicketSolver.Framework.Application;

public interface IChatService
{
    Task<string> CreateTicketAsync(CancellationToken cancellationToken, TicketDTO ticketDto);
}
