using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Domain.Services.Tenant.Interfaces;

public interface ITenantsService
{
    Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken);
    
    Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken);
    
    Task<bool> IsTenantExistsAsync(string key, CancellationToken cancellationToken);
    
    Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken);
}