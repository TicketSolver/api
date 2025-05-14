using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Domain.Repositories.User;

namespace TicketSolver.Application.Services.Ticket;

public class TicketsService(ITicketsRepository repo, IUsersRepository usersRepo) : ITicketsService
{
    public Task<IEnumerable<Tickets>> GetAllAsync() => repo.GetAllAsync();

    public Task<Tickets?> GetByIdAsync(int id) => repo.GetByIdAsync(id);

    public async Task<Tickets> CreateAsync(Tickets ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        return await repo.AddAsync(ticket);
    }

    public async Task<bool> UpdateAsync(int id, Tickets ticket)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return false;
        existing.Title = ticket.Title;
        existing.Description = ticket.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing is null)
            return false;

        await repo.DeleteAsync(existing);
        return true;
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, short status)
    {
        var existing = await repo.GetByIdAsync(id);
        if (status < 0 || status > 3)
            return false;
        if (existing is null)
            return false;
        existing.Status = status;
        existing.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> AssignedTechTicketAsync(int id, string techId)
    {
        var existing = await repo.GetByIdAsync(id);
        var existingTech = usersRepo.GetById(techId);
        if ((existing is null) || (existingTech.FirstOrDefault() is null))
            return false;
        existing.AssignedToId = techId;
        existing.UpdatedAt = DateTime.UtcNow;
        await repo.UpdateAsync(existing);
        return true;
    }

    public async Task<IEnumerable<Tickets>> GetAllByUserAsync(string id)
    {
        var existing = usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return tickets;
        tickets = await repo.GetAllByUserAsync(id);
        return tickets;
    }

    public async Task<IEnumerable<Tickets>> GetAllByTechAsync(string id)
    {
        var existing = usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return tickets;
        tickets = await repo.GetAllByTechAsync(id);
        return tickets;
    }

    public async Task<string> GetCountsasync(string id)
    {
        var existing = usersRepo.GetById(id);
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult("0");
        var countAll = await repo.GetCountsasync(id);
        var coutnInProgress = await repo.GetCountsasync(id, 0);
        var countWaiting = await repo.GetCountsasync(id, 1);
        var countResolved = await repo.GetCountsasync(id, 2);
        if (countAll == 0)
        {
            return """
                total: 0,
                 inProgress: 0,
                 waiting: 0,
                 resolved: 0 
                """;
        }
        else
            return $"""
                total: {countAll},
                 inProgress: {coutnInProgress},
                 waiting: {countWaiting},
                 resolved: {countResolved} 
                """;
    }

    public async Task<IEnumerable<Tickets>> GetLatestUserAsync(string id)
    {
        var existing = usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult(tickets);
        tickets = await repo.GetLatestUserAsync(id);
        return await Task.FromResult(tickets);
    }

    public async Task<IEnumerable<Tickets>> GetLatestTechAsync(string id)
    {
        var existing = usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult(tickets);
        tickets = await repo.GetLatestTechAsync(id);
        return await Task.FromResult(tickets);
    }
}
