using System;
using System.Linq;
using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class APIKey{
    public async Task GroqTaskq()
    {
        var apiKey = Environment.GetEnvironmentVariable("Ai__Groq__ApiKey");
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
                services.AddGroqClient(apiKey, GroqModel.LLaMA3_8b);
            })
            .Build();


        var groqClient = host.Services.GetRequiredService<GroqClient>();

        var history = new GroqChatHistory
        {
            new("What is the capital of France?")
        };
        
        var result = await groqClient.GetChatCompletionsAsync(history);

        Console.WriteLine(result.Choices.First().Message.Content);
        Console.WriteLine($"Total tokens used: {result.Usage.TotalTokens}; Time to response: {result.Usage.TotalTime} sec.");
        
        await foreach (var msg in groqClient.GetChatCompletionsStreamingAsync(history))
        {
            Console.WriteLine(msg.Choices[0].Delta.Content);

            if (msg?.XGroq?.Usage != null)
            {
                Console.WriteLine($"Total tokens used: {msg?.XGroq?.Usage.TotalTokens}; Time to response: {msg?.XGroq?.Usage.TotalTime} sec.");
            }
        }
    }
}
