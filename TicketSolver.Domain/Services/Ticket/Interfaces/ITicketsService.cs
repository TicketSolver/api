namespace TicketSolver.Domain.Services.Ticket.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using TicketSolver.Domain.Persistence.Tables.Ticket;



public interface ITicketsService
{
    Task<IEnumerable<Tickets>> GetAllAsync();
    Task<Tickets?> GetByIdAsync(int id);
    Task<Tickets> CreateAsync(Tickets ticket);
    Task<bool> UpdateAsync(int id, Tickets ticket);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateTicketStatusAsync(int id, short status);
    Task<bool> AssignedTechTicketAsync (int id, int techId);
    Task<IEnumerable<Tickets>> GetAllByUserAsync(int id);
    Task<IEnumerable<Tickets>> GetAllByTechAsync(int id);
    Task<string> GetCountsasync(int id);
    Task<IEnumerable<Tickets>> GetLatestUserAsync(int id);
    Task<IEnumerable<Tickets>> GetLatestTechAsync(int id);
}
