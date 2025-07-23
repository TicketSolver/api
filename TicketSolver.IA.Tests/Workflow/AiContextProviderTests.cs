using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Framework.Domain;
using TicketSolver.Application.Models;
using TicketSolver.Api.Framework;
using TicketSolver.Application.Interfaces;
using TicketSolver.Application.Ports;
using TicketSolver.Application.Services;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;

namespace TicketSolver.IA.Tests.Workflow
{
    public class AiContextProviderTests
    {
        [Fact]
        public async Task GetAiContext_ContextExists_ReturnsFromRepository()
        {
            // Arrange
            var tenant = new Tenants { ApplicationType = ApplicationType.Web };
            var existing = new AiContext("repo-prompt");

            var repo = new Mock<IAiContextRepository>();
            repo.Setup(r => r.GetContext(
                    It.IsAny<CancellationToken>(),
                    It.Is<Tenants>(t => t.ApplicationType == ApplicationType.Web)
                ))
                .ReturnsAsync(existing);

            var external = new Mock<IExternalInfoService>();

            var provider = new AiContextProvider(repo.Object, external.Object);

            // Act
            var result = await provider.GetAiContext(tenant, CancellationToken.None);

            // Assert
            Assert.Equal(existing, result);
            external.Verify(e => e.GetContext(It.IsAny<CancellationToken>(), It.IsAny<Tenants>()), Times.Never);
            repo.Verify(r => r.AddAiContext(It.IsAny<CancellationToken>(), It.IsAny<AiContext>(), It.IsAny<Tenants>()), Times.Never);
        }

        [Fact]
        public async Task GetAiContext_ContextMissing_FetchesAndStores()
        {
            // Arrange
            var tenant = new Tenants { ApplicationType = ApplicationType.Mobile };
            var externalCtx = new AiContext("external-prompt");

            var repo = new Mock<IAiContextRepository>();
            repo.Setup(r => r.GetContext(
                    It.IsAny<CancellationToken>(),
                    It.Is<Tenants>(t => t.ApplicationType == ApplicationType.Mobile)
                ))
                .ReturnsAsync((AiContext?)null);
            repo.Setup(r => r.AddAiContext(
                    It.IsAny<CancellationToken>(),
                    externalCtx,
                    It.Is<Tenants>(t => t.ApplicationType == ApplicationType.Mobile)
                ))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var external = new Mock<IExternalInfoService>();
            external.Setup(e => e.GetContext(
                    It.IsAny<CancellationToken>(),
                    It.Is<Tenants>(t => t.ApplicationType == ApplicationType.Mobile)
                ))
                .ReturnsAsync(externalCtx)
                .Verifiable();

            var provider = new AiContextProvider(repo.Object, external.Object);

            // Act
            var result = await provider.GetAiContext(tenant, CancellationToken.None);

            // Assert
            Assert.Equal(externalCtx, result);
            external.Verify();
            repo.Verify();
        }
    }
}
