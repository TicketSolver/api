using TicketSolver.Application.Models;
using TicketSolver.Application.Models.User;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Models.Ticket;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Services.Ticket.Interfaces;

public interface ITicketsService
{
    Task<IEnumerable<TicketShort>> GetAllAsync();
    Task<Tickets?> GetByIdAsync(int id);
    Task<Tickets> CreateAsync(TicketDTO ticket, string userId);
    Task<Tickets> UpdateAsync(TicketDTO ticket, int id);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateTicketStatusAsync(int id, short status);
    Task<bool> AssignedTechTicketAsync(CancellationToken cancellationToken, int ticketId, string techId);

    Task<PaginatedResponse<Tickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId,
        PaginatedQuery paginatedQuery);
    Task<PaginatedResponse<Tickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery, bool history = false);
    Task<TechnicianPerformance> GetTechPerformanceAsync(CancellationToken cancellationToken, string techId);
    Task<TechnicianCounters> GetTechCountersAsync(CancellationToken cancellationToken, string techId);
    Task<string> GetCountsasync(string id);
    Task<IEnumerable<Tickets>> GetLatestUserAsync(string id);
    Task<IEnumerable<Tickets>> GetLatestTechAsync(string id);
}