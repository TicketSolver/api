using System.Threading;
using System.Threading.Tasks;
using Moq;
using TicketSolver.Application.Models;
using TicketSolver.Application.Ports;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow;

public class BaseChatServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_UsesContextAndReturnsAiResponse()
    {
        // Arrange
        var tenant = new Tenant("1");
        var aiContext = new AiContext("system-prompt");

        var contextProvider = new Mock<IAiContextProvider>();
        contextProvider.Setup(p => p.GetAiContext(tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiContext);

        var aiProvider = new Mock<IAiProvider>();
        string? capturedPrompt = null;
        aiProvider.Setup(p => p.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("ai-response");

        var service = new BaseChatService(contextProvider.Object, aiProvider.Object);
        var ticketDto = new TicketDTO
        {
            Title = "Issue title",
            Description = "Issue desc",
            Category = short.Parse(tenant.Id)
        };

        // Act
        var result = await service.CreateTicketAsync(CancellationToken.None, ticketDto);

        // Assert
        Assert.Equal("ai-response", result);
        Assert.NotNull(capturedPrompt);
        Assert.Contains(aiContext.SystemPrompt, capturedPrompt);
        Assert.Contains(ticketDto.Title, capturedPrompt!);
        Assert.Contains(ticketDto.Description, capturedPrompt!);

        contextProvider.Verify(p => p.GetAiContext(tenant, It.IsAny<CancellationToken>()), Times.Once);
        aiProvider.Verify(p => p.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
