using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Application.Interfaces;

public interface IExternalInfoService
{
    Task<AiContext> GetContext(CancellationToken cancellationToken, Tenants tenant);
}
