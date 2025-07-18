using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;
using TicketSolver.Api.Application.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Api.Application;

public class AiContextProvider : IAiContextProvider
{
    private readonly IAiContextRepository _repository;
    private readonly IExternalInfoService _externalInfo;

    public AiContextProvider(
        IAiContextRepository repository,
        IExternalInfoService externalInfo)
    {
        _repository = repository;
        _externalInfo = externalInfo;
    }

    public async Task<AiContext> GetAiContext(Tenants tenant, CancellationToken cancellationToken)
    {
        var ctx = await _repository.GetContext(cancellationToken, tenant);
        if (ctx is null)
        {
            ctx = await _externalInfo.GetContext(cancellationToken, tenant);
            await _repository.AddAiContext(cancellationToken, ctx, tenant);
        }
        return ctx;
    }
}
