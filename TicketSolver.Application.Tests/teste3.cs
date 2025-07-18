// GroqClientTests.cs

using System;
using Microsoft.Extensions.Configuration;
using DotNetEnv;            // ➊
using System.Threading.Tasks;
using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TicketSolver.Application.Tests;

public class GroqClientTests
{
    private readonly GroqClient _client;
    
    public GroqClientTests()
    {
        Env.Load();

        var apiKey = 
            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build()["Ai:Groq:ApiKey"]
            ?? Environment.GetEnvironmentVariable("Ai__Groq__ApiKey")
            ?? throw new InvalidOperationException("API Key não encontrada");

        // 1) Build a ServiceCollection just like your HostBuilder does
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddGroqClient(apiKey, GroqModel.LLaMA3_8b);

        // 2) Build the provider and grab the client
        var provider = services.BuildServiceProvider();
        _client = provider.GetRequiredService<GroqClient>();
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

        // 3) Act
        var chatResult = await _client.GetChatCompletionsAsync(history);

        // 4) Assert
        Assert.NotNull(chatResult);
        Assert.NotNull(chatResult.Choices);
        Assert.NotEmpty(chatResult.Choices);
        Assert.False(string.IsNullOrWhiteSpace(chatResult.Choices[0].Message.Content));
    }
}