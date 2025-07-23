using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;
using TicketSolver.Application.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Application.Services;

public class AiContextProvider(
    IAiContextRepository repository,
    IExternalInfoService externalInfo)
    : IAiContextProvider
{
    public async Task<AiContext> GetAiContext(Tenants tenant, CancellationToken cancellationToken)
    {
        var ctx = await repository.GetContext(cancellationToken, tenant);
        if (ctx is null)
        {
            ctx = await externalInfo.GetContext(cancellationToken, tenant);
            await repository.AddAiContext(cancellationToken, ctx, tenant);
        }
        return ctx;
    }
}
