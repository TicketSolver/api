using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Repositories.Tenant;

public interface ITenantsRepository : IRepositoryBase<Tenants>
{
    Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Tenants?> GetTenantByKeyAsync(Guid key, CancellationToken cancellationToken);

    Task<int> GetTypeKey(Guid key, CancellationToken cancellationToken);
    
    Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken);
    
    Task<bool> IsTenantExistsAsync(Guid key, CancellationToken cancellationToken);


    Task<Tenants?> GetByPublicKeyAsync(Guid tenantKey, CancellationToken ct);
    
    Task<Tenants?> GetByAdminKeyAsync(Guid tenantKey, CancellationToken ct);
}