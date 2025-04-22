using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Db.Tables;


namespace TicketSolver.Domain.Persistence;

public class EFContext(DbContextOptions<EFContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
}