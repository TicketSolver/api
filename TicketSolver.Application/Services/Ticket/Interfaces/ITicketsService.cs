using TicketSolver.Application.Models;
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
    Task<bool> AssignedTechTicketAsync(int id, string techId);
    Task<IEnumerable<Tickets>> GetAllByUserAsync(string id);
    Task<IEnumerable<Tickets>> GetAllByTechAsync(string id);
    Task<string> GetCountsasync(string id);
    Task<IEnumerable<Tickets>> GetLatestUserAsync(string id);
    Task<IEnumerable<Tickets>> GetLatestTechAsync(string id);
}