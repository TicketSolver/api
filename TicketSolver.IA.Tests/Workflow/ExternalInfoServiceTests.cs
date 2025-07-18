using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Api.Infra;
using TicketSolver.Framework.Domain;
using TicketSolver.Api.Infra.SystemPrompts;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow;

public class ExternalInfoServiceTests
{
    [Theory]
    [InlineData("mobile", MobileSystemPrompt.Text)]
    [InlineData("web", WebSystemPrompt.Text)]
    [InlineData("enterprise", EnterpriseSystemPrompt.Text)]
    public async Task GetContext_ReturnsPromptBasedOnTenant(string tenantId, string expected)
    {
        var service = new ExternalInfoService();
        var tenant = new Tenant(tenantId);

        var ctx = await service.GetContext(CancellationToken.None, tenant);

        Assert.Equal(expected, ctx.SystemPrompt);
    }
}
