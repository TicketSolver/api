using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Infra.EntityFramework.Persistence.Mappings;

public static class TicketMappings
{
    public static void MapTickets(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketUsers>()
            .HasOne(tu => tu.Ticket)
            .WithMany(t => t.TicketUsers)
            .HasForeignKey(tu => tu.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketUsers>()
            .HasOne<Users>(tu => tu.User)
            .WithMany(u => u.TicketUsers)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketUpdates>()
            .HasOne(tu => tu.Ticket)
            .WithMany(t => t.TicketUpdates)
            .HasForeignKey(tu => tu.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attachments>()
            .HasOne(tu => tu.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(tu => tu.TicketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}