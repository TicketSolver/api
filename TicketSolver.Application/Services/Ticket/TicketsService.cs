using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models;
using TicketSolver.Application.Models.User;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Models.Ticket;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Domain.Repositories.User;

namespace TicketSolver.Application.Services.Ticket;

public class TicketsService(
    ITicketsRepository repo,
    IUsersRepository usersRepo,
    ITicketUsersRepository ticketUsersRepository,
    ITicketsRepository ticketsRepository
) : ITicketsService
{
    public async Task<IEnumerable<TicketShort>> GetAllAsync()
    {
        var tickets = await repo.GetAllAsync();

        var ticketShorts = tickets.Select(ticket => new TicketShort
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.DefTicketPriorityId,
            Category = ticket.DefTicketCategoryId,
            Created = ticket.CreatedAt,
            Updated = ticket.UpdatedAt,
            CreatedBy = ticket.CreatedBy == null
                ? null
                : new UserShort
                {
                    Email = ticket.CreatedBy.Email,
                    FullName = ticket.CreatedBy.FullName,
                    UserTypeId = ticket.CreatedBy.DefUserTypeId,
                    Id = ticket.CreatedBy.Id
                },
        }).ToList();

        return ticketShorts;
    }


    public Task<Tickets?> GetByIdAsync(int id) => repo.GetByIdAsync(id);

    public async Task<Tickets> CreateAsync(TicketDTO ticket, string userId)
    {
        Tickets t = new Tickets();
        t.CreatedById = userId;
        t.Title = ticket.Title;
        t.Description = ticket.Description;
        t.DefUserSatisfactionId = 1;
        t.Status = (short)eDefTicketStatus.New;
        t.DefTicketPriorityId = ticket.Priority;
        t.DefTicketCategoryId = ticket.Category;
        t.CreatedAt = DateTime.UtcNow;
        t.UpdatedAt = DateTime.UtcNow;
        return await repo.AddAsync(t);
    }

    public async Task<Tickets> UpdateAsync(TicketDTO ticket, int id)
    {
        Tickets updateAsync;
        try
        {
            var newTicket = new Tickets
            {
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                DefTicketPriorityId = ticket.Priority,
                DefTicketCategoryId = ticket.Category
            };
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                throw new TicketException("Ticket não encontrado");
            existing.Title = newTicket.Title;
            existing.Description = newTicket.Description;
            existing.Status = newTicket.Status;
            existing.DefTicketPriorityId = newTicket.DefTicketPriorityId;
            existing.DefTicketCategoryId = newTicket.DefTicketCategoryId;
            existing.UpdatedAt = DateTime.UtcNow;
            updateAsync = await repo.UpdateAsync(existing) ?? new Tickets();
        }
        catch (TicketException e)
        {
            throw new TicketException("Erro ao criar o ticket");
        }


        if (updateAsync is null)
            throw new TicketException("Erro ao atualizar o ticket");
        return updateAsync;
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

    public async Task<bool> AssignedTechTicketAsync(CancellationToken cancellationToken, int ticketId, string techId)
    {
        var ticketExists = await repo.GetById(ticketId).AnyAsync(cancellationToken);
        if (!ticketExists)
            throw new TicketNotFoundException();

        var technicianExists = await usersRepo.GetById(techId).AnyAsync(cancellationToken);
        if (!technicianExists)
            throw new UserNotFoundException();

        var ticketUser = new TicketUsers()
        {
            UserId = techId,
            TicketId = ticketId
        };

        return await ticketUsersRepository.InsertAsync(cancellationToken, ticketUser);
    }

    public async Task<PaginatedResponse<Tickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId,
        PaginatedQuery paginatedQuery)
    {
        var userExists = await usersRepo.GetById(userId).AnyAsync(cancellationToken);
        if (!userExists)
            throw new UserNotFoundException();
        
        return await repo.GetAllByUserAsync(cancellationToken, userId, paginatedQuery);
    }

    public async Task<PaginatedResponse<Tickets>> GetAllByTechAsync(CancellationToken cancellationToken, string id,
        PaginatedQuery paginatedQuery, bool history = false)
    {
        var userExists = await usersRepo.GetById(id).AnyAsync(cancellationToken);
        if (!userExists)
            throw new UserNotFoundException();

        if (history)
            return await repo.GetHistoryByTechAsync(cancellationToken, id, paginatedQuery);
        
        return await repo.GetAllByTechAsync(cancellationToken, id, paginatedQuery);
    }

    public async Task<TechnicianPerformance> GetTechPerformanceAsync(CancellationToken cancellationToken, string techId)
    {
        var query = ticketUsersRepository.GetAll()
            .AsNoTracking()
            .Where(tu => tu.UserId == techId);

        var techHasTicket = await query.AnyAsync(cancellationToken);

        var totalTickets = techHasTicket
            ? await query
                .Select(tu => tu.TicketId)
                .Distinct()
                .CountAsync(cancellationToken)
            : 0;

        var resolvedTickets = techHasTicket
            ? await query
                .Where(tu => tu.UserId == techId)
                .Select(tu => tu.Ticket)
                .Where(t => t.Status == (short)eDefTicketStatus.Closed)
                .CountAsync(cancellationToken)
            : 0;

        var satisfaction = techHasTicket
            ? await query
                .AverageAsync(f => f.Ticket.DefUserSatisfactionId / 5.0 * 100, cancellationToken)
            : 0;

        var responseTime = await query
            .Where(tu => tu.FirstResponseAt != null)
            .Select(tu => new { tu.FirstResponseAt, tu.CreatedAt })
            .ToListAsync(cancellationToken);

        var performance = new TechnicianPerformance
        {
            SolvingPercentage = totalTickets == 0 ? 0 : (resolvedTickets * 100.0) / totalTickets,
            Satisfaction = satisfaction,
            AnwserTime = responseTime.Count == 0
                ? 0
                : responseTime.Average(r => (r.FirstResponseAt!.Value - r.CreatedAt).TotalMinutes),
        };

        return performance;
    }

    public async Task<TechnicianCounters> GetTechCountersAsync(CancellationToken cancellationToken, string techId)
    {
        var today = DateTime.UtcNow.Date;

        var result = await ticketUsersRepository.GetByUserId(techId)
            .AsNoTracking()
            .GroupBy(tu => 1)
            .Select(g => new TechnicianCounters
            {
                CurrentlyWorking = g.Count(tu => tu.Ticket.SolvedAt == null),
                Critical = g.Count(tu => tu.Ticket.SolvedAt == null && tu.Ticket.DefTicketPriorityId == (short)eDefTicketPriorities.Urgent),
                HighPriority = g.Count(tu => tu.Ticket.SolvedAt == null && tu.Ticket.DefTicketPriorityId == (short)eDefTicketPriorities.High),
                SolvedToday = g.Count(tu => tu.Ticket.SolvedAt != null && tu.Ticket.SolvedAt >= today)
            })
            .FirstOrDefaultAsync(cancellationToken) ?? new TechnicianCounters();

        return result;
    }

    public async Task<UserCounters> GetUserCountersAsync(CancellationToken cancellationToken, string userId)
    {
        var result = await ticketsRepository.GetAll()
            .AsNoTracking()
            .Where(t => t.CreatedById == userId)
            .GroupBy(t => 1)
            .Select(g => new UserCounters
            {
                Open = g.Count(t => t.SolvedAt == null),
                InProgress = g.Count(t => t.Status == (short)eDefTicketStatus.InProgress),
                Resolved = g.Count(t => t.Status == (short)eDefTicketStatus.Resolved),
                Total = g.Count(),
            })
            .FirstOrDefaultAsync(cancellationToken) ?? new UserCounters();

        
        return result;
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