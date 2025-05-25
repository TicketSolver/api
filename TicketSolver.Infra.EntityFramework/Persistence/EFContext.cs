using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Infra.EntityFramework.Persistence.Mappings;

namespace TicketSolver.Infra.EntityFramework.Persistence;

public class EFContext(DbContextOptions<EFContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.MapTickets();
        modelBuilder.MapUsers();
    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<TicketUsers> TicketUsers { get; set; }
    public DbSet<TicketUpdates> TicketUpdates { get; set; }
    public DbSet<Attachments> Attachments { get; set; }

    public DbSet<Tenants> Tenants { get; set; }

    public DbSet<IdentityUser> IdentityUsers { get; set; }
    public DbSet<IdentityRole> IdentityRoles { get; set; }

    public DbSet<DefTicketUserRoles> DefTicketUserRoles { get; set; }
    public DbSet<DefTicketCategories> DefTicketCategories { get; set; }
    public DbSet<DefTicketPriorities> DefTicketPriorities { get; set; }
    public DbSet<DefTicketStatus> DefTicketStatus { get; set; }
    public DbSet<DefUserTypes> DefUserTypes { get; set; }
    public DbSet<DefUserStatus> DefUserStatus { get; set; }
    public DbSet<DefStorageProviders> DefStorageProviders { get; set; }
}