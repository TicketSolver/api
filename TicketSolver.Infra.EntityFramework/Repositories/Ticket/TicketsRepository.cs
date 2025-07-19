using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class TicketsRepository<TTickets>(
    IEfContext context) : EFRepositoryBase<TTickets>(context), ITicketsRepository<TTickets>
    where TTickets : Tickets
{
    private readonly DbSet<TTickets> dbSet = context.Set<TTickets>();

    public virtual async Task<IEnumerable<TTickets>> GetAllAsync()
        => await dbSet
            .Include(t => t.CreatedBy)
            .ToListAsync();

    public virtual async Task<TTickets> AddAsync(TTickets ticket)
    {
        dbSet.Add(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public virtual async Task<TTickets?> UpdateAsync(TTickets ticket)
    {
        context.Entry(ticket).State = EntityState.Modified;
        await context.SaveChangesAsync();
        var updatedTicket = await dbSet
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);
        return updatedTicket;
    }


    public virtual async Task DeleteAsync(TTickets ticket)
    {
        dbSet.Remove(ticket);
        await context.SaveChangesAsync();
    }

    public virtual async Task<PaginatedResponse<TTickets>> GetAllByUserAsync(CancellationToken cancellationToken, string userId,
        PaginatedQuery paginatedQuery)
    {
        var query = dbSet
            .Include(t => t.CreatedBy)
            .Where(t => t.CreatedById == userId);

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public virtual async Task<PaginatedResponse<TTickets>> GetAllByTechAsync(CancellationToken cancellationToken, string techId,
        PaginatedQuery paginatedQuery)
    {
        var query = dbSet
            .Where(t => t.TicketUsers.Any(tu => tu.UserId == techId));

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public virtual async Task<PaginatedResponse<TTickets>> GetHistoryByTechAsync(CancellationToken cancellationToken,
        string techId,
        PaginatedQuery paginatedQuery)
    {
        var query = dbSet
            .Where(t => t.Status == (short)eDefTicketStatus.Closed && t.TicketUsers.Any(tu => tu.UserId == techId));

        return await ToPaginatedResult(cancellationToken, query, paginatedQuery);
    }

    public virtual async Task<int> GetCountsasync(string id, int status)
    {
        var coutnInProgress = await dbSet
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id && t.Status == status)
            .CountAsync();
        return coutnInProgress;
    }

    public virtual async Task<int> GetCountsasync(string id)
    {
        var coutnInProgress = await dbSet
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id)
            .CountAsync();
        return coutnInProgress;
    }

    public virtual async Task<IEnumerable<TTickets>> GetLatestUserAsync(string id)
    {
        return await dbSet
            .Include(t => t.CreatedById)
            .Where(t => t.CreatedById == id)
            .OrderByDescending(t => t.CreatedAt).Take(5)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<TTickets>> GetLatestTechAsync(string id)
    {
        return await dbSet
            .Where(t => t.TicketUsers.Any(tu => tu.UserId == id))
            .OrderByDescending(t => t.CreatedAt).Take(5)
            .ToListAsync();
    }

    public virtual Task<bool> ExistsAsync(int requestTicketId, CancellationToken cancellationToken)
    {
        return dbSet
            .AnyAsync(t => t.Id == requestTicketId, cancellationToken);
    }

    public virtual async Task<TTickets?> GetByIdAsync(int id)
    {
        return await DbSet
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public virtual IQueryable<TTickets> GetById(int id)
    {
        return DbSet
            .Include(t => t.CreatedBy)
            .Where(t => t.Id == id);
    }

    // Novos métodos
    public virtual async Task UpdateStatusAsync(int ticketId, eDefTicketStatus status,
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

    public virtual async Task<IEnumerable<TTickets>> GetTicketsWithUnreadUserMessagesAsync(
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