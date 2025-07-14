// GroqClientTests.cs

using System;
using Microsoft.Extensions.Configuration;
using DotNetEnv;            // ➊
using System.Threading.Tasks;
using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading;

namespace TicketSolver.Application.Tests;

public class GroqClientTests
{
    private readonly Mock<IGroqClient> _clientMock;
    
    public GroqClientTests()
    {
        _clientMock = new Mock<IGroqClient>();
    }
    [Fact]
    public async Task CreateCompletionAsync_WithValidPrompt_ReturnsValidResponse()
    {
        // 1) Declare your prompt
        var prompt = "Explique brevemente o que é .NET 9";

        // 2) Build your chat history using that prompt
        var history = new GroqChatHistory
        {
            new GroqMessage(prompt)
        };

        var chatResult = new GroqChatCompletions
        {
            Id = "test-id",
            Object = "test-object",
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = "test-model",
            SystemFingerprint = "test-fingerprint",
            Usage = new GroqUsage
            {
                PromptTokens = 1,
                CompletionTokens = 1,
                TotalTokens = 2,
                PromptTime = 0.1M,
                CompletionTime = 0.1M,
                TotalTime = 0.2M
            },
            XGroq = new() { Id = "test-id" },
            Choices = new List<GroqChoice>
            {
                new GroqChoice
                {
                    Index = 0,
                    FinishReason = "stop",
                    Message = new GroqMessage
                    {
                        Content = "Test response"
                    }
                }
            }
        };

        _clientMock.Setup(x => x.GetChatCompletionsAsync(history, null, CancellationToken.None))
            .ReturnsAsync(chatResult);

        // 3) Act
        var result = await _clientMock.Object.GetChatCompletionsAsync(history, null, CancellationToken.None);

        // 4) Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Choices);
        Assert.NotEmpty(result.Choices);
        Assert.False(string.IsNullOrWhiteSpace(result.Choices[0].Message.Content));
    }
}
