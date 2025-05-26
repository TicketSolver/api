using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketSolver.Application.Services.ChatAI;
using TicketSolver.Application.Services.ChatAI.Interface;

namespace TicketSolver.Infra.GeminiAI;

class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("Ai__Groq__ApiKey");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
                services.AddGroqClient(apiKey!, GroqModel.LLaMA3_8b);

                // registra o ChatService
                services.AddSingleton<IChatAiService, AiChatService>();
            })
            .Build();

        var chatService = host.Services.GetRequiredService<IChatAiService>();

        // Exemplo de uso:
        var history = new GroqChatHistory();
        string systemPrompt = "Você é um assistente especializado em DDD.";
        string userPrompt   = "Explique o padrão Repository.";

        var reply = await chatService.AskAsync(history, userPrompt, systemPrompt);
        Console.WriteLine("IA respondeu:");
        Console.WriteLine(reply);
    }
}