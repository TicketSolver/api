using System;
using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Api.Infra;
using TicketSolver.Api.Infra.SystemPrompts;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow
{
    public class ExternalInfoServiceTests
    {
        [Theory]
        [InlineData(ApplicationType.Mobile, MobileSystemPrompt.Text)]
        [InlineData(ApplicationType.Web, WebSystemPrompt.Text)]
        [InlineData(ApplicationType.Enterprise, EnterpriseSystemPrompt.Text)]
        public async Task GetContext_ReturnsPromptBasedOnTenant(ApplicationType appType, string expected)
        {
            // Arrange
            var service = new ExternalInfoService();
            var tenant = new Tenants
            {
                Id              = 1,
                Name            = "TestTenant",
                AdminKey        = Guid.NewGuid(),
                PublicKey       = Guid.NewGuid(),
                IsConfigured    = true,
                ApplicationType = appType
            };

            // Act
            var ctx = await service.GetContext(CancellationToken.None, tenant);

            // Assert
            Assert.Equal(expected, ctx.SystemPrompt);
        }
    }
}