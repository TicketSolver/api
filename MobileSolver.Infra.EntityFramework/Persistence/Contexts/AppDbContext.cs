using Microsoft.EntityFrameworkCore;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace MobileSolver.Infra.EntityFramework.Persistence.Contexts;

public class AppDbContext(DbContextOptions<EfContext> options) : EfContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Tickets>();
        modelBuilder.Ignore<Users>();
    }

    public DbSet<MobileTickets> MobileTickets { get; set; }
    public DbSet<CustomUsers> CustomUsers { get; set; }
}