
using Moq;
using TicketSolver.Application.interfaces;
using TicketSolver.Application.Models;
using TicketSolver.Application.Ports;
using TicketSolver.Application.Services;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Domain;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow;

public class BaseChatServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_UsesContextAndReturnsAiResponse()
    {
        // Arrange
        var ticketDto = new TicketDTO
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Category = (short)ApplicationType.Web
        };

        var aiContext = new AiContext("ctx-prompt");
        var contextProvider = new Mock<IAiContextProvider>();
        contextProvider
            .Setup(p => p.GetAiContext(
                It.Is<Tenants>(t => t.ApplicationType == ApplicationType.Web),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(aiContext)
            .Verifiable();

        string captured = null;
        var aiProvider = new Mock<IAiProvider>();
        aiProvider
            .Setup(p => p.GenerateTextAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<string, CancellationToken>((prompt, ct) => captured = prompt)
            .ReturnsAsync("ai-response")
            .Verifiable();

        var service = new BaseChatService(contextProvider.Object, aiProvider.Object);

        // Act
        var result = await service.CreateTicketAsync(CancellationToken.None, ticketDto);

        // Assert
        Assert.Equal("ai-response", result);
        Assert.NotNull(captured);
        Assert.Contains(aiContext.SystemPrompt, captured);
        Assert.Contains(ticketDto.Title, captured);
        Assert.Contains(ticketDto.Description, captured);

        contextProvider.Verify();
        aiProvider.Verify();
    }
}
