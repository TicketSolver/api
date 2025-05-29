using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Tenant;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Infra.EntityFramework.Repositories.Tenant;

public class TenantsRepository(EfContext context) : EFRepositoryBase<Tenants>(context), ITenantsRepository
{
    public override IQueryable<Tenants> GetAll()
    {
        return base.GetAll().AsNoTracking();
    }

    public async Task<IEnumerable<Tenants>> ExecuteQueryAsync(IQueryable<Tenants> query, CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetAll().ToListAsync(cancellationToken);
    }
    public async Task<Tenants?> GetTenantByKeyAsync(Guid key, CancellationToken cancellationToken)
    {
        return await GetAll()
            .Where(t => t.AdminKey == key || t.PublicKey == key)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> GetTypeKey(Guid key, CancellationToken cancellationToken)
    {
        return Context.Tenants
            .Where(t => t.AdminKey == key || t.PublicKey == key)
            .Select(t => t.AdminKey == key ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);
    }



    public async Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        await Context.Tenants.AddAsync(tenant, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return tenant;
    }
    
    public async Task<bool> IsTenantExistsAsync(Guid key, CancellationToken cancellationToken)
    {
        return await GetAll()
            .AnyAsync(t => t.AdminKey == key || t.PublicKey == key, cancellationToken);
    }

    public Task<Tenants?> GetByPublicKeyAsync(Guid tenantKey, CancellationToken ct)
    {
        return GetAll()
            .Where(t => t.PublicKey == tenantKey)
            .FirstOrDefaultAsync(ct);
    }
    
    public Task<Tenants?> GetByAdminKeyAsync(Guid tenantKey, CancellationToken ct)
    {
        return GetAll()
            .Where(t => t.AdminKey == tenantKey)
            .FirstOrDefaultAsync(ct);
    }
    
}