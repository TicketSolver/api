using TicketSolver.Application.interfaces;
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
        if (ctx is not null) return ctx;
        ctx = await externalInfo.GetContext(cancellationToken, tenant);
        await repository.AddAiContext(cancellationToken, ctx, tenant);
        return ctx;
    }
}
