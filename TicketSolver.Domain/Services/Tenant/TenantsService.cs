using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Repositories.Tenant.Interfaces;
using TicketSolver.Domain.Services.Tenant.Interfaces;

namespace TicketSolver.Domain.Services.Tenant;

public class TenantsService(
    ITenantsRepository tenantsRepository
) : ITenantsService
{
}