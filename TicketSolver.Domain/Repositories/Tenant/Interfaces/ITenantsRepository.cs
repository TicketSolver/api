using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Interfaces;

namespace TicketSolver.Domain.Repositories.Tenant.Interfaces;

public interface ITenantsRepository : IEFRepositoryBase<Tenants>
{
    Task<Tenants?> GetTenantByKeyAsync(string key, CancellationToken cancellationToken); 
}