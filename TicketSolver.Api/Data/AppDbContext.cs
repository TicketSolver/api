namespace TicketSolver.Api.Data;

using Microsoft.EntityFrameworkCore;
using TicketSolver.Api.Models;

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        
    }