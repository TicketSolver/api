// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using TicketSolver.Api.Framework;
// using TicketSolver.Application.Models;
// using TicketSolver.Application.Services.Chat.Interfaces;
// using TicketSolver.Application.Services.Tenant.Interfaces;
// using TicketSolver.Domain.Persistence.Tables.Tenant;
// using TicketSolver.Framework.Domain;
// using Xunit;
//
// namespace TicketSolver.IA.Tests.Workflow
// {
//     public class FrameworkChatControllerTests
//     {
//         [Fact]
//         public async Task AddAiContextAsync_CallsChatServiceAndReturnsResult()
//         {
//             // Arrange
//             var tenantKey = Guid.NewGuid();
//             var tenant = new Tenants { Id = 1, ApplicationType = ApplicationType.Enterprise };
//
//             var tenantServiceMock = new Mock<ITenantsService>();
//             tenantServiceMock
//                 .Setup(s => s.GetTenantByKeyAsync(tenantKey, It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(tenant);
//
//             var chatServiceMock = new Mock<IChatService>();
//             chatServiceMock
//                 .Setup(s => s.CreateTicketAsync(
//                     It.IsAny<CancellationToken>(),
//                     It.Is<TicketDTO>(dto => dto.Category == (short)ApplicationType.Enterprise)))
//                 .ReturnsAsync("chat-response")
//                 .Verifiable();
//
//             var aiContextRepoMock = new Mock<IAiContextRepository>();
//
//             // Act
//             var controller = new ChatController(
//             );
//
//             var result = await controller
//                     .AddAiContextAsync(tenantKey, "any-prompt", CancellationToken.None)
//                 as OkObjectResult;
//
//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal("chat-response", result.Value);
//             chatServiceMock.Verify();
//         }
//     }
// }