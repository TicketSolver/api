using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Interfaces;

namespace TicketSolver.Domain.Repositories.Tenant.Interfaces;

public interface ITenantsRepository : IEFRepositoryBase<Tenants>
{
    Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken);
    
    Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken);
    
    Task<bool> IsTenantExistsAsync(string key, CancellationToken cancellationToken);
    
}