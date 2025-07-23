using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Framework.Application;

public interface IAiContextProvider
{
    Task<AiContext> GetAiContext(Tenants tenant, CancellationToken cancellationToken);
}
