﻿
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketSolver.Api.Controllers;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.ChatAI.Interface;
using Xunit;
using GroqNet;                      // para GroqChatHistory
using GroqNet.ChatCompletions;      // caso seu controller inclua esse namespace

namespace TicketSolver.IA.Tests
{
    public class ChatControllerTests
    {
        private readonly Mock<IChatAiService> _mockChatService;
        private readonly ChatAiController _controller;

        public ChatControllerTests()
        {
            _mockChatService = new Mock<IChatAiService>(MockBehavior.Strict);
            _controller = new ChatAiController(_mockChatService.Object);
        }

        [Fact]
        public async Task Ask_EmptyPrompt_ReturnsBadRequest()
        {
            // Arrange
            // record ChatRequest(Guid? ConversationId, string Prompt, string? SystemPrompt)
            var request = new ChatRequest(
                ConversationId: null,
                Prompt:         "   ",
                SystemPrompt:   null
            );

            // Act
            var result = await _controller.Ask(request, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("O campo 'Prompt' não pode estar vazio.", badRequest.Value);
        }

        [Fact(DisplayName = "Ask: prompt válido deve chamar o serviço e retornar Ok")]
        public async Task Ask_ValidPrompt_CallsServiceAndReturnsOk()
        {
            // Arrange
            var convId    = Guid.NewGuid();
            var fakeReply = "Resposta de teste";
            var request   = new ChatRequest(
                ConversationId: convId,
                Prompt:        "Olá, IA!",
                SystemPrompt:  "Você é XY"
            );

            // desambiguamos aqui usando a sobrecarga genérica
            _mockChatService
                .Setup<Task<string>>(s => s.AskAsync(
                    It.IsAny<GroqChatHistory>(),
                    request.Prompt,
                    request.SystemPrompt
                ))
                .ReturnsAsync(fakeReply);

            // Act
            var actionResult = await _controller.Ask(request, CancellationToken.None);

            // Assert
            var okModel = Assert.IsType<OkObjectResult>(actionResult);
            var dto     = Assert.IsType<ChatResponse>(okModel.Value);

            Assert.Equal(convId,         dto.ConversationId);
            Assert.Equal(fakeReply,      dto.Reply);
            _mockChatService.Verify(s => s.AskAsync(
                It.IsAny<GroqChatHistory>(),
                request.Prompt,
                request.SystemPrompt
            ), Times.Once);
        }

        [Fact(DisplayName = "Ask sem ConversationId deve gerar novo Guid")]
        public async Task Ask_NoConversationId_GeneratesNewGuid()
        {
            // Arrange
            var fakeReply = "Outra resposta";
            var request   = new ChatRequest(
                ConversationId: null,
                Prompt:        "Teste sem convId",
                SystemPrompt:  null
            );

            _mockChatService
                .Setup<Task<string>>(s => s.AskAsync(
                    It.IsAny<GroqChatHistory>(),
                    request.Prompt,
                    (string?)null
                ))
                .ReturnsAsync(fakeReply);

            // Act
            var actionResult = await _controller.Ask(request, CancellationToken.None);

            // Assert
            var okModel = Assert.IsType<OkObjectResult>(actionResult);
            var dto     = Assert.IsType<ChatResponse>(okModel.Value);

            Assert.NotEqual(Guid.Empty, dto.ConversationId);
            Assert.Equal(fakeReply,      dto.Reply);
            _mockChatService.Verify(s => s.AskAsync(
                It.IsAny<GroqChatHistory>(),
                request.Prompt,
                (string?)null
            ), Times.Once);
        }
    }
}
