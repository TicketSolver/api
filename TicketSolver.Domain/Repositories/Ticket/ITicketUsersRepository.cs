using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Ticket;

public interface ITicketUsersRepository : IRepositoryBase<TicketUsers>
{
    Task<bool> IsUserAssignedToTicketAsync(CancellationToken cancellationToken, string userId, int ticketId);
}