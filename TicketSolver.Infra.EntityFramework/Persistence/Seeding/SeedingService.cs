using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Infra.EntityFramework.Persistence.Seeding;

public class SeedingService(EfContext context, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedAsync()
    {
        await DefsSeeding.SeedDefsAsync(context);
        await SeedTenantsAsync();
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { eDefUserTypes.Admin, eDefUserTypes.Technician, eDefUserTypes.Client };
    
        foreach (var role in roles.Select(r => r.ToString()))
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedTenantsAsync()
    {
        if (await context.Tenants.AnyAsync()) return;

        List<string> tenantsNames = ["Test", "TicketSolver"];
        var tenants = tenantsNames.Select(tenantName => new Tenants()
        {
            Name = tenantName,
            AdminKey = Guid.NewGuid(),
            PublicKey = Guid.NewGuid(),
            IsConfigured = false,
        }).ToList();

        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();
    }
}