using GroqNet;
using TicketSolver.Application.Services.ChatAI.Interface;
using GroqNet.ChatCompletions;

namespace TicketSolver.Application.Services.ChatAI;

public class AiChatService(GroqClient groq) : IChatAiService
{
    public async Task<string> AskAsync(
        GroqChatHistory history,
        string prompt,
        string? systemPrompt = null
    )
    {
        try
        {
            // Remove a última mensagem se for igual ao prompt (evita duplicação)
            if (history.Any() && history.Last().Content == prompt && history.Last().Role == "user")
            {
                history.RemoveAt(history.Count - 1);
            }

            // 1) Se houver systemPrompt e for a primeira mensagem, adiciona
            if (!string.IsNullOrWhiteSpace(systemPrompt) && !history.Any(m => m.Role == "system"))
            {
                history.Insert(0, new GroqMessage(systemPrompt) { Role = "system" });
            }

            // 2) Adiciona o prompt atual
            history.Add(new GroqMessage(prompt) { Role = "user" });

            Console.WriteLine($"=== ENVIANDO PARA GROQ ===");
            Console.WriteLine($"Total de mensagens: {history.Count}");
            foreach (var msg in history)
            {
                Console.WriteLine($"Role: {msg.Role} | Content: {msg.Content?.Substring(0, Math.Min(50, msg.Content.Length))}...");
            }

            // 3) Chama o GroqClient
            var response = await groq.GetChatCompletionsAsync(history);
            return response.Choices.First().Message.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro detalhado do Groq: {ex}");
            throw;
        }
    }


    public async Task<string> AskWithHistoryAsync(GroqChatHistory history)
    {
        try
        {
            Console.WriteLine($"=== ENVIANDO PARA GROQ ===");
            Console.WriteLine($"Total de mensagens: {history.Count}");
        
            foreach (var msg in history)
            {
                Console.WriteLine($"Role: {msg.Role} | Content: {msg.Content?.Substring(0, Math.Min(50, msg.Content.Length))}...");
            }
        
            var response = await groq.GetChatCompletionsAsync(history);
            return response.Choices.First().Message.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no Groq: {ex.Message}");
            throw;
        }
    }
    public async Task<string> AskWithFullHistoryAsync(GroqChatHistory history)
    {
        try
        {
            Console.WriteLine($"=== ENVIANDO PARA GROQ ===");
            Console.WriteLine($"Total de mensagens: {history.Count}");
        
            foreach (var msg in history)
            {
                Console.WriteLine($"Role: {msg.Role} | Content: {msg.Content?.Substring(0, Math.Min(100, msg.Content?.Length ?? 0))}...");
            }
        
            var response = await groq.GetChatCompletionsAsync(history);
            return response.Choices.First().Message.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no Groq: {ex.Message}");
            throw;
        }
    }

}