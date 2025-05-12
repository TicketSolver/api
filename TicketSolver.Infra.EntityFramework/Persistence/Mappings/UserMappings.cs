using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Infra.EntityFramework.Persistence.Mappings;

public static class UserMappings
{
    public static void MapUsers(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>()
            .HasOne<Tenants>(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}