using Microsoft.EntityFrameworkCore;
using RemoteSolver.Domain.Persistence.Entities;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace RemoteSolver.Infra.EntityFramework.Persistence.Contexts;

public class AppDbContext(DbContextOptions<EfContext> options) : EfContext(options), IEfContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Tickets>();
    }

    public DbSet<RemoteTickets> RemoteTickets { get; set; }
}
