using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Ticket;

public interface ITicketsRepository<TTickets> : IRepositoryBase<TTickets>
{
    Task<IEnumerable<TTickets>> GetAllAsync();
    Task<TTickets?> GetByIdAsync(int id);
    Task<TTickets> AddAsync(TTickets ticket);
    Task<TTickets?> UpdateAsync(TTickets ticket);
    Task DeleteAsync(TTickets ticket);
    Task UpdateStatusAsync(int ticketId, eDefTicketStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TTickets>> GetTicketsWithUnreadUserMessagesAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResponse<TTickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId, PaginatedQuery paginatedQuery);
    
    Task<PaginatedResponse<TTickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery);
    Task<PaginatedResponse<TTickets>> GetHistoryByTechAsync(CancellationToken cancellationToken, string techId, PaginatedQuery paginatedQuery);

    Task<int> GetCountsasync(string id, int status);
    Task<int> GetCountsasync(string id);

     Task<IEnumerable<TTickets>> GetLatestUserAsync(string id);
     Task<IEnumerable<TTickets>> GetLatestTechAsync(string id);
     Task<bool> ExistsAsync(int requestTicketId, CancellationToken cancellationToken);
}
