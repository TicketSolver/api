using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Application.Services.Tenant.Interfaces;

public interface ITenantsService
{
    Task<Tenants?> GetTenantByKeyAsync(Guid key, CancellationToken cancellationToken);
    
    Task<Tenants?> AddTenantAsync(Tenants tenant, CancellationToken cancellationToken);
    
    Task<bool> TenantExistsAsync(Guid key, CancellationToken cancellationToken);
    
    Task<IEnumerable<Tenants>> GetAllAsync(CancellationToken cancellationToken);
}