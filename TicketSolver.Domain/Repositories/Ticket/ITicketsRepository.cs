using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Ticket;

public interface ITicketsRepository
{
    Task<IEnumerable<Tickets>> GetAllAsync();
    Task<Tickets?> GetByIdAsync(int id);
    Task<Tickets> AddAsync(Tickets ticket);
    Task UpdateAsync(Tickets ticket);
    Task DeleteAsync(Tickets ticket);

    Task<IEnumerable<Tickets>> GetAllByUserAsync(string id);
    
    Task<IEnumerable<Tickets>> GetAllByTechAsync(string id);

    Task<int> GetCountsasync(string id, int status);
    Task<int> GetCountsasync(string id);

     Task<IEnumerable<Tickets>> GetLatestUserAsync(string id);
     Task<IEnumerable<Tickets>> GetLatestTechAsync(string id);
}
