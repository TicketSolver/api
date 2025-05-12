using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Domain.Repositories.Tenant;

public interface ITenantsRepository
{
    Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Tenants?> GetTenantByKeyAsync(Guid key, CancellationToken cancellationToken);
    
    Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken);
    
    Task<bool> IsTenantExistsAsync(Guid key, CancellationToken cancellationToken);
    
}