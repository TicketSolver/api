using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Application.interfaces;

public interface IAiContextProvider
{
    Task<AiContext> GetAiContext(Tenants tenant, CancellationToken cancellationToken);
}
