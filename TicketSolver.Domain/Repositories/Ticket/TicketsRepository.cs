using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Ticket.Interfaces;

namespace TicketSolver.Domain.Repositories.Ticket;

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

  public async Task<IEnumerable<Tickets>> GetAllByUserAsync(int id)
  {
    return await context.Tickets
      .Include(t => t.CreatedById)
      .Where(t => t.CreatedById == id)
      .ToListAsync();
  }

  public async Task<IEnumerable<Tickets>> GetAllByTechAsync(int id)
  {
    return  await context.Tickets
      .Include(t => t.AssignedToId)
      .Where(t => t.AssignedToId == id)
      .ToListAsync();
  }

  public async Task<int> GetCountsasync(int id, int status)
  {
        var coutnInProgress = await context.Tickets
          .Include(t => t.CreatedById)
          .Where(t => t.CreatedById == id && t.Status == status)
          .CountAsync();
      return coutnInProgress;
  }

  public async Task<int> GetCountsasync(int id)
  {
    var coutnInProgress = await context.Tickets
      .Include(t => t.CreatedById)
      .Where(t => t.CreatedById == id)
      .CountAsync();
    return coutnInProgress; 
  }

  public async Task<IEnumerable<Tickets>> GetLatestUserAsync(int id)
  {
    return await context.Tickets
      .Include(t => t.CreatedById)
      .Where(t => t.CreatedById == id)
      .OrderByDescending(t => t.CreatedAt).Take(5)
      .ToListAsync();
  }
  
  public async Task<IEnumerable<Tickets>> GetLatestTechAsync(int id)
  {
    return await context.Tickets
      .Include(t => t.AssignedToId)
      .Where(t => t.AssignedToId == id)
      .OrderByDescending(t => t.CreatedAt).Take(5)
      .ToListAsync();
  }
}
