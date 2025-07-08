using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Application.Interfaces;

public interface IExternalInfoService
{
    Task<AiContext> GetContext(CancellationToken cancellationToken, Tenant tenant);
}
