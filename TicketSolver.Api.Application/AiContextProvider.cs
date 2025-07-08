using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;
using TicketSolver.Api.Application.Interfaces;

namespace TicketSolver.Api.Application;

public class AiContextProvider(
    IAiContextRepository repository,
    IExternalInfoService externalInfo)
    : IAiContextProvider
{
    public async Task<AiContext> GetAiContext(Tenant tenant)
    {
        var ctx = await repository.GetContext(CancellationToken.None, tenant);
        if (ctx is null)
        {
            ctx = await externalInfo.GetContext(CancellationToken.None, tenant);
            await repository.AddAiContext(CancellationToken.None, ctx, tenant);
        }

        return ctx;
    }
}
