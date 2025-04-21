using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;

namespace TicketSolver.Domain.Repositories.Tenant;

public class TenantsRepository(EFContext context) : EFRepositoryBase<Tenants>(context), ITenantsRepository
{
    public async Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken)
    {
        return await GetAll()
            .Where(t => t.AdminKey == key || t.PublicKey == key)
            .FirstOrDefaultAsync(cancellationToken);
    } 
}