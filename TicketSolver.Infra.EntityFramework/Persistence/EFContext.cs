using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Chat;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.Service;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;
using TicketSolver.Infra.EntityFramework.Persistence.Mappings;

namespace TicketSolver.Infra.EntityFramework.Persistence;

public class EfContext(DbContextOptions<EfContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.MapTickets();
        modelBuilder.MapUsers();
        modelBuilder.Entity<TicketChat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChatHistory)
                  .HasColumnType("jsonb") // PostgreSQL JSONB
                  .IsRequired()
                  .HasDefaultValue("[]");
            entity.Property(e => e.TotalMessages)
                  .HasDefaultValue(0);
            entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                  .IsRequired()
                  .HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastMessageAt)
                  .HasColumnType("timestamp without time zone");
            entity.HasOne(e => e.Ticket)
                  .WithOne() // ou .WithOne(t => t.Chat) se adicionar no Ticket
                  .HasForeignKey<TicketChat>(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_TicketChats_Tickets");
            entity.HasIndex(e => e.TicketId)
                  .IsUnique() // Garante 1:1
                  .HasDatabaseName("IX_TicketChats_TicketId");
            entity.HasIndex(e => e.LastMessageAt)
                  .HasDatabaseName("IX_TicketChats_LastMessageAt");
            entity.HasIndex(e => e.ChatHistory)
                  .HasMethod("gin")
                  .HasDatabaseName("IX_TicketChats_ChatHistory_Gin");
        });
        
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<Tickets> Tickets { get; set; }
    public DbSet<TicketUsers> TicketUsers { get; set; }
    public DbSet<TicketUpdates> TicketUpdates { get; set; }
    public DbSet<Attachments> Attachments { get; set; }
    public DbSet<TicketChat> Chats { get; set; }
    
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
    public DbSet<DefUserSatisfaction> DefUserSatisfaction { get; set; }
    public DbSet<ServiceAddress> ServiceAddresses { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<ServiceAvailableSlots> ServiceAvailableSlots { get; set; }
    public override int SaveChanges()
    {
          UpdateTimestamps();
          return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
          UpdateTimestamps();
          return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
          var entries = ChangeTracker.Entries<TicketChat>()
                .Where(e => e.State == EntityState.Modified);

          foreach (var entry in entries)
          {
                entry.Entity.UpdatedAt = DateTime.Now;
          }
    }
}