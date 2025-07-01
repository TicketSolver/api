using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Chat;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

public interface IEfContext : IDbContext
{
    DbSet<Users> Users { get; set; }
    DbSet<Tickets> Tickets { get; set; }
    DbSet<TicketUsers> TicketUsers { get; set; }
    DbSet<TicketUpdates> TicketUpdates { get; set; }
    DbSet<Attachments> Attachments { get; set; }
    DbSet<TicketChat> Chats { get; set; }
    DbSet<Tenants> Tenants { get; set; }

    DbSet<IdentityUser> IdentityUsers { get; set; }
    DbSet<IdentityRole> IdentityRoles { get; set; }

    DbSet<DefTicketUserRoles> DefTicketUserRoles { get; set; }
    DbSet<DefTicketCategories> DefTicketCategories { get; set; }
    DbSet<DefTicketPriorities> DefTicketPriorities { get; set; }
    DbSet<DefTicketStatus> DefTicketStatus { get; set; }
    DbSet<DefUserTypes> DefUserTypes { get; set; }
    DbSet<DefUserStatus> DefUserStatus { get; set; }
    DbSet<DefStorageProviders> DefStorageProviders { get; set; }
    DbSet<DefUserSatisfaction> DefUserSatisfaction { get; set; }

    DbSet<ServiceAddress> ServiceAddresses { get; set; }
    DbSet<ServiceRequest> ServiceRequests { get; set; }
    DbSet<ServiceAvailableSlots> ServiceAvailableSlots { get; set; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
