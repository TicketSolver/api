using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Extensions;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class TicketsRepository(EfContext context) : EFRepositoryBase<Tickets>(context), ITicketsRepository
{
    public async Task<IEnumerable<Tickets>> GetAllAsync()
        => await context.Tickets
            .Include(t => t.CreatedBy)
            .ToListAsync();

    public async Task<Tickets> AddAsync(Tickets ticket)
    {
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Tickets?> UpdateAsync(Tickets ticket)
    {
        context.Entry(ticket).State = EntityState.Modified;
        await context.SaveChangesAsync();
        var updatedTicket = await context.Tickets
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);
        return updatedTicket;
    }


    public async Task DeleteAsync(Tickets ticket)
    {
        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<Tickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId,
        PaginatedQuery paginatedQuery)
    {
        var query = context.Tickets
            .Include(t => t.CreatedBy)
            .Where(t => t.CreatedById == userId);

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public async Task<PaginatedResponse<Tickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId,
        PaginatedQuery paginatedQuery)
    {
        var query = context.Tickets
            .Where(t => t.TicketUsers.Any(tu => tu.UserId == techId));

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public async Task<PaginatedResponse<Tickets>> GetHistoryByTechAsync(CancellationToken cancellationToken,
        string techId,
        PaginatedQuery paginatedQuery)
    {
        var query = context.Tickets
            .Where(t => t.Status == (short)eDefTicketStatus.Closed && t.TicketUsers.Any(tu => tu.UserId == techId));

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public async Task<int> GetCountsasync(string id, int status)
    {
        var coutnInProgress = await context.Tickets
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id && t.Status == status)
            .CountAsync();
        return coutnInProgress;
    }

    public async Task<int> GetCountsasync(string id)
    {
        var coutnInProgress = await context.Tickets
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id)
            .CountAsync();
        return coutnInProgress;
    }

    public async Task<IEnumerable<Tickets>> GetLatestUserAsync(string id)
    {
        return await context.Tickets
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id)
            .OrderByDescending(t => t.CreatedAt).Take(5)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tickets>> GetLatestTechAsync(string id)
    {
        return await context.Tickets
            .Where(t => t.TicketUsers.Any(tu => tu.UserId == id))
            .OrderByDescending(t => t.CreatedAt).Take(5)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(int requestTicketId, CancellationToken cancellationToken)
    {
        return context.Tickets
            .AnyAsync(t => t.Id == requestTicketId, cancellationToken);
    }

    public async Task<Tickets?> GetByIdAsync(int id)
    {
        return await DbSet
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public IQueryable<Tickets> GetById(int id)
    {
        return DbSet
            .Include(t => t.CreatedBy)
            .Where(t => t.Id == id);
    }

    // Novos métodos
    public async Task UpdateStatusAsync(int ticketId, eDefTicketStatus status,
        CancellationToken cancellationToken = default)
    {
        var ticket = await DbSet.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket != null)
        {
            ticket.Status = (short)status;
            ticket.UpdatedAt = DateTime.Now;

            DbSet.Update(ticket);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Tickets>> GetTicketsWithUnreadUserMessagesAsync(
        CancellationToken cancellationToken = default)
    {
        var ticketsWithTechnicians = await (
                from ticket in DbSet
                join ticketUser in Context.TicketUsers
                    on ticket.Id equals ticketUser.TicketId
                where ticketUser.DefTicketUserRoleId == (short)eDefTicketUserRoles.Responder &&
                      (ticket.Status == (short)eDefTicketStatus.InProgress ||
                       ticket.Status == (short)eDefTicketStatus.New)
                select ticket
            ).Include(t => t.CreatedBy)
            .Include(t => t.TicketUsers)
            .ToListAsync(cancellationToken);

        return ticketsWithTechnicians;
    }
}