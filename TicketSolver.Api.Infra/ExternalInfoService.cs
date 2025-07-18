using TicketSolver.Framework.Domain;
using TicketSolver.Api.Application.Interfaces;
using TicketSolver.Api.Infra.SystemPrompts;

namespace TicketSolver.Api.Infra;

public class ExternalInfoService : IExternalInfoService
{
    public Task<AiContext> GetContext(CancellationToken cancellationToken, Tenant tenant)
    {
        var prompt = tenant.Id.ToLowerInvariant() switch
        {
            "mobile" => MobileSystemPrompt.Text,
            "web"    => WebSystemPrompt.Text,
            _         => EnterpriseSystemPrompt.Text
        };

        return Task.FromResult(new AiContext(prompt));
    }
}
