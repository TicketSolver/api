using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence;

public class EFContext(DbContextOptions<EFContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    
    public DbSet<Tenants> Tenants { get; set; }
}