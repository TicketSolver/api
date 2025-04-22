using TicketSolver.Domain.Persistence.Tables.Ticket;
namespace TicketSolver.Domain.Repositories.Ticket.Interfaces;

public interface ITicketsRepository
{
    Task<IEnumerable<Tickets>> GetAllAsync();
    Task<Tickets?> GetByIdAsync(int id);
    Task<Tickets> AddAsync(Tickets ticket);
    Task UpdateAsync(Tickets ticket);
    Task DeleteAsync(Tickets ticket);

    Task<IEnumerable<Tickets>> GetAllByUserAsync(int id);
    
    Task<IEnumerable<Tickets>> GetAllByTechAsync(int id);

    Task<int> GetCountsasync(int id, int status);
    Task<int> GetCountsasync(int id);

     Task<IEnumerable<Tickets>> GetLatestUserAsync(int id);
     Task<IEnumerable<Tickets>> GetLatestTechAsync(int id);
}
