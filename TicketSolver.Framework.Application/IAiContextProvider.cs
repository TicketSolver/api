using TicketSolver.Framework.Domain;

namespace TicketSolver.Framework.Application;

public interface IAiContextProvider
{
    Task<AiContext> GetAiContext(Tenant tenant);
}
