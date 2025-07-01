using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Ticket;

public interface ITicketUsersRepository : IRepositoryBase<TicketUsers>
{
    Task<bool> IsUserAssignedToTicketAsync(CancellationToken cancellationToken, string userId, int ticketId);
    IQueryable<TicketUsers> GetByUserId(string userId);
    Task UnassignUserToTicketAsync(CancellationToken cancellationToken, string userId, int ticketId);
    Task<bool> HasTechnicianAssignedAsync(int ticketId, CancellationToken cancellationToken = default);
    Task AssignTechnicianToTicketAsync(int ticketId, string technicianId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAvailableTechniciansAsync(CancellationToken cancellationToken = default);
    Task<string> GetAssignedTechnicianNameAsync(int ticketId, CancellationToken cancellationToken = default);
    
}