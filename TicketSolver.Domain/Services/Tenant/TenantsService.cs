using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;
using TicketSolver.Domain.Services.Tenant.Interfaces;

namespace TicketSolver.Domain.Services.Tenant;

public class TenantsService(
    ITenantsRepository tenantsRepository
) : ITenantsService
{
    public async Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken){
        return await tenantsRepository.GetTenantByKeyAsync(key, cancellationToken);
    }

    public async Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken){
        return await tenantsRepository.AddTenantAsync(tenant, cancellationToken);
    }

    public async Task<bool> IsTenantExistsAsync(string key, CancellationToken cancellationToken)
    {
        return await tenantsRepository.IsTenantExistsAsync(key, cancellationToken);
    }
    
    public async Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await tenantsRepository.GetAllAsync(cancellationToken);
    }
}