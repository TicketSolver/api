using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Framework.Domain;

public interface IAiContextRepository
{
    Task<AiContext?> GetContext(CancellationToken cancellationToken, Tenants tenant);
    Task AddAiContext(CancellationToken cancellationToken, AiContext context, Tenants tenant);
}
