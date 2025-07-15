using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketSolver.Api.Framework;
using TicketSolver.Application.Models;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;
using Xunit;

namespace TicketSolver.IA.Tests.Workflow;

public class FrameworkChatControllerTests
{
    [Fact]
    public async Task StartChatAsync_ReturnsOkWithServiceResult()
    {
        var chatService = new Mock<IChatService>();
        chatService.Setup(s => s.CreateTicketAsync(It.IsAny<CancellationToken>(), It.IsAny<TicketDTO>()))
            .ReturnsAsync("ok");
        var repo = new Mock<IAiContextRepository>();

        var controller = new ChatController(chatService.Object, repo.Object);

        var ticket = new TicketDTO { Title = "t", Description = "d", Category = 1 };
        var result = await controller.StartChatAsync(ticket, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("ok", ok.Value);
        chatService.Verify(s => s.CreateTicketAsync(It.IsAny<CancellationToken>(), ticket), Times.Once);
    }

    [Fact]
    public async Task AddAiContextAsync_PersistsContext()
    {
        var chatService = new Mock<IChatService>();
        var repo = new Mock<IAiContextRepository>();
        repo.Setup(r => r.AddAiContext(It.IsAny<CancellationToken>(), It.IsAny<AiContext>(), It.IsAny<Tenant>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var controller = new ChatController(chatService.Object, repo.Object);

        var result = await controller.AddAiContextAsync("web", "prompt", CancellationToken.None);

        Assert.IsType<OkResult>(result);
        repo.Verify(r => r.AddAiContext(
            It.IsAny<CancellationToken>(),
            It.Is<AiContext>(c => c.SystemPrompt == "prompt"),
            It.Is<Tenant>(t => t.Id == "web")), Times.Once);
    }
}
