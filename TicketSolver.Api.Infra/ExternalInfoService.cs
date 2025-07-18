using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Api.Application.Interfaces;
using TicketSolver.Api.Infra.SystemPrompts;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Infra
{
    public class ExternalInfoService : IExternalInfoService
    {
        public Task<AiContext> GetContext(CancellationToken cancellationToken, Tenants tenant)
        {
            var prompt = tenant.ApplicationType switch
            {
                ApplicationType.Mobile     => MobileSystemPrompt.Text,
                ApplicationType.Web        => WebSystemPrompt.Text,
                ApplicationType.Enterprise => EnterpriseSystemPrompt.Text
            };

            return Task.FromResult(new AiContext(prompt));
        }
    }
}