using TicketSolver.Application.Models;
using TicketSolver.Application.Models.User;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Models.Ticket;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Services.Ticket.Interfaces;

public interface ITicketsService<TTickets> where TTickets : Tickets
{
    Task<IEnumerable<TicketShort>> GetAllAsync();
    Task<TTickets?> GetByIdAsync(int id);
    Task<TTickets> CreateAsync(TicketDTO ticketDto, string userId);
    Task<TTickets> UpdateAsync(TicketDTO ticket, int id);
    Task<bool> DeleteAsync(int id);
    Task<List<AssignedUser>> GetTicketUsersAsync(CancellationToken cancellationToken, int id);
    Task<List<UserDto>> GetAvailableTicketUsersAsync(CancellationToken cancellationToken, int id);
    Task<bool> UpdateTicketStatusAsync(int id, short status);
    Task<bool> AssignedTechTicketAsync(CancellationToken cancellationToken, int ticketId, HashSet<string> techsId);
    Task UnassignTechAsync(CancellationToken cancellationToken, int ticketId, string techId);

    Task<PaginatedResponse<TTickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId,
        PaginatedQuery paginatedQuery);
    Task<PaginatedResponse<TTickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery, bool history = false);
    Task<TechnicianPerformance> GetTechPerformanceAsync(CancellationToken cancellationToken, string techId);
    Task<TechnicianCounters> GetTechCountersAsync(CancellationToken cancellationToken, string techId);
    Task<UserCounters> GetUserCountersAsync(CancellationToken cancellationToken, string user);
    Task<string> GetCountsasync(string id);
    Task<IEnumerable<TTickets>> GetLatestUserAsync(string id);
    Task<IEnumerable<TTickets>> GetLatestTechAsync(string id);
}