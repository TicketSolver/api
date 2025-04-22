using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;

namespace TicketSolver.Domain.Repositories.Tenant;

public class TenantsRepository(EFContext context) : EFRepositoryBase<Tenants>(context), ITenantsRepository
{
    public async Task<IEnumerable<Tenants>> GetAllAsync()
    {
        return await GetAll().ToListAsync();
    }
    public async Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken)
    {
        return await GetAll()
            .Where(t => t.AdminKey == key || t.PublicKey == key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken)
    {
        await context.Tenants.AddAsync(tenant, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return tenant;
    }
    
    public async Task<bool> IsTenantExistsAsync(string key, CancellationToken cancellationToken)
    {
        return await GetAll()
            .AnyAsync(t => t.AdminKey == key || t.PublicKey == key, cancellationToken);
    }
}