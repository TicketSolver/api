namespace TicketSolver.Framework.Domain;

public interface IAiContextRepository
{
    Task<AiContext?> GetContext(CancellationToken cancellationToken, Tenant tenant);
    Task AddAiContext(CancellationToken cancellationToken, AiContext context, Tenant tenant);
}
