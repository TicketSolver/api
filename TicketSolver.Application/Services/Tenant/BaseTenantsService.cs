using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Tenant;

namespace TicketSolver.Application.Services.Tenant;

public class BaseTenantsService(
    ITenantsRepository tenantsRepository
) : ITenantsService
{
    public async Task<Tenants?> GetTenantByKeyAsync(Guid key, CancellationToken cancellationToken)
    {
        return await tenantsRepository.GetTenantByKeyAsync(key, cancellationToken);
    }

    public async Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken){
        return await tenantsRepository.AddTenantAsync(tenant, cancellationToken);
    }

    public async Task<bool> TenantExistsAsync(Guid key, CancellationToken cancellationToken)
    {
        return await tenantsRepository.IsTenantExistsAsync(key, cancellationToken);
    }
    
    public async Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await tenantsRepository.GetAllAsync(cancellationToken);
    }
}