using System.Threading;
using System.Threading.Tasks;
using Moq;
using TicketSolver.Api.Application;
using TicketSolver.Api.Application.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow;

public class AiContextProviderTests
{
    [Fact]
    public async Task GetAiContext_ContextExists_ReturnsFromRepository()
    {
        // Arrange
        var tenant = new Tenants("web");
        var existing = new AiContext("prompt");

        var repo = new Mock<IAiContextRepository>();
        repo.Setup(r => r.GetContext(It.IsAny<CancellationToken>(), tenant))
            .ReturnsAsync(existing);

        var external = new Mock<IExternalInfoService>();

        var provider = new AiContextProvider(repo.Object, external.Object);

        // Act
        var result = await provider.GetAiContext(tenant, CancellationToken.None);

        // Assert
        Assert.Equal(existing, result);
        external.Verify(e => e.GetContext(It.IsAny<CancellationToken>(), It.IsAny<Tenant>()), Times.Never);
        repo.Verify(r => r.AddAiContext(It.IsAny<CancellationToken>(), It.IsAny<AiContext>(), It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task GetAiContext_ContextMissing_FetchesAndStores()
    {
        // Arrange
        var tenant = new Tenants("mobile");
        var externalCtx = new AiContextProvider("external");

        var repo = new Mock<IAiContextRepository>();
        repo.Setup(r => r.GetContext(It.IsAny<CancellationToken>(), tenant))
            .ReturnsAsync((AiContext?)null);
        repo.Setup(r => r.AddAiContext(It.IsAny<CancellationToken>(), externalCtx, tenant))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var external = new Mock<IExternalInfoService>();
        external.Setup(e => e.GetContext(It.IsAny<CancellationToken>(), tenant))
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
