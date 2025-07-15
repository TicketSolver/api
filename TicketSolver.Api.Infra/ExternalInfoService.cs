using TicketSolver.Framework.Domain;
using TicketSolver.Api.Application.Interfaces;
using TicketSolver.Api.Infra.SystemPrompts;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.Api.Infra;

public class ExternalInfoService : IExternalInfoService
{
    public Task<AiContext> GetContext(CancellationToken cancellationToken, Tenants tenant)
    {
        var prompt = tenant.ApplicationType switch
        {
            ApplicationType.Mobile     => MobileSystemPrompt.Text,
            ApplicationType.Web        => WebSystemPrompt.Text,
            _                           => EnterpriseSystemPrompt.Text
        };

        return Task.FromResult(new AiContext(prompt));
    }

    public Task<AiContext> GetContext(CancellationToken cancellationToken, Tenant tenant)
    {
        throw new NotImplementedException();
    }
}
