using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Ticket;

public class TicketsRepository(EFContext context) : ITicketsRepository
{
  public async Task<IEnumerable<Tickets>> GetAllAsync()
    => await context.Tickets.ToListAsync();

  public async Task<Tickets?> GetByIdAsync(int id)
    => await context.Tickets.FindAsync(id);

  public async Task<Tickets> AddAsync(Tickets ticket)
  {
    context.Tickets.Add(ticket);
    await context.SaveChangesAsync();
    return ticket;
  }

  public async Task UpdateAsync(Tickets ticket)
  {
    context.Entry(ticket).State = EntityState.Modified;
    await context.SaveChangesAsync();
  }

  public async Task DeleteAsync(Tickets ticket)
  {
    context.Tickets.Remove(ticket);
    await context.SaveChangesAsync();
  }

  public async Task<IEnumerable<Tickets>> GetAllByUserAsync(string id)
  {
    return await context.Tickets
      .Include(t => t.CreatedById)
      .Where(t => t.CreatedById == id)
      .ToListAsync();
  }

  public async Task<IEnumerable<Tickets>> GetAllByTechAsync(string id)
  {
    return  await context.Tickets
      .Include(t => t.AssignedToId)
      .Where(t => t.AssignedToId == id)
      .ToListAsync();
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
      .Include(t => t.AssignedToId)
      .Where(t => t.AssignedToId == id)
      .OrderByDescending(t => t.CreatedAt).Take(5)
      .ToListAsync();
  }
}
