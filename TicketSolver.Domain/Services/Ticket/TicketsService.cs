using TicketSolver.Domain.Repositories.User.Interfaces;

namespace TicketSolver.Domain.Services.Ticket.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket.Interfaces;
using TicketSolver.Domain.Services.Ticket.Interfaces;

public class TicketsService : ITicketsService
{
    private readonly ITicketsRepository _repo;
    private readonly IUsersRepository _usersRepo;

    public TicketsService(ITicketsRepository repo, IUsersRepository usersRepo)
    {
        _usersRepo = usersRepo;
        _repo = repo;
    }

    public Task<IEnumerable<Tickets>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Tickets?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<Tickets> CreateAsync(Tickets ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        return await _repo.AddAsync(ticket);
    }

    public async Task<bool> UpdateAsync(int id, Tickets ticket)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return false;
        existing.Title = ticket.Title;
        existing.Description = ticket.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
            return false;

        await _repo.DeleteAsync(existing);
        return true;
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, short status)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (status < 0 || status > 3)
            return false;
        if (existing is null)
            return false;
        existing.Status = status;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> AssignedTechTicketAsync(int id, int techId)
    {
        var existing = await _repo.GetByIdAsync(id);
        var existingTech = _usersRepo.GetById(techId);
        if ((existing is null) || (existingTech.FirstOrDefault() is null))
            return false;
        existing.AssignedToId = techId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing);
        return true;
    }

    public async Task<IEnumerable<Tickets>> GetAllByUserAsync(int id)
    {
        var existing = _usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return tickets;
        tickets = await _repo.GetAllByUserAsync(id);
        return tickets;
    }

    public async Task<IEnumerable<Tickets>> GetAllByTechAsync(int id)
    {
        var existing = _usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return tickets;
        tickets = await _repo.GetAllByTechAsync(id);
        return tickets;
    }

    public async Task<string> GetCountsasync(int id)
    {
        var existing = _usersRepo.GetById(id);
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult("0");
        int countAll = await _repo.GetCountsasync(id);
        int coutnInProgress = await _repo.GetCountsasync(id, 0);
        int countWaiting = await _repo.GetCountsasync(id, 1);
        int countResolved = await _repo.GetCountsasync(id, 2);
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

    public async Task<IEnumerable<Tickets>> GetLatestUserAsync(int id)
    {
        var existing = _usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult(tickets);
        tickets = await _repo.GetLatestUserAsync(id);
        return await Task.FromResult(tickets);
    }

    public async Task<IEnumerable<Tickets>> GetLatestTechAsync(int id)
    {
        var existing = _usersRepo.GetById(id);
        IEnumerable<Tickets> tickets = new List<Tickets>();
        if (existing.FirstOrDefault() is null)
            return await Task.FromResult(tickets);
        tickets = await _repo.GetLatestTechAsync(id);
        return await Task.FromResult(tickets);
    }
}
