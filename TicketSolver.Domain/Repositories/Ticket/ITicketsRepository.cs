using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Ticket;

public interface ITicketsRepository : IRepositoryBase<Tickets>
{
    Task<IEnumerable<Tickets>> GetAllAsync();
    Task<Tickets?> GetByIdAsync(int id);
    Task<Tickets> AddAsync(Tickets ticket);
    Task<Tickets?> UpdateAsync(Tickets ticket);
    Task DeleteAsync(Tickets ticket);
    Task UpdateStatusAsync(int ticketId, eDefTicketStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tickets>> GetTicketsWithUnreadUserMessagesAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Tickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId, PaginatedQuery paginatedQuery);
    
    Task<PaginatedResponse<Tickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery);
    Task<PaginatedResponse<Tickets>> GetHistoryByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery);

    Task<int> GetCountsasync(string id, int status);
    Task<int> GetCountsasync(string id);

     Task<IEnumerable<Tickets>> GetLatestUserAsync(string id);
     Task<IEnumerable<Tickets>> GetLatestTechAsync(string id);
     Task<bool> ExistsAsync(int requestTicketId, CancellationToken cancellationToken);
}
