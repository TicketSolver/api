using GroqNet;
using TicketSolver.Application.Services.ChatAI.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;
using GroqNet.ChatCompletions;

namespace TicketSolver.Application.Services.ChatAI
{
    public class AiChatService : IChatAiService
    {
        private readonly GroqClient _groq;
        public AiChatService(GroqClient groq) => _groq = groq;

        public async Task<string> AskAsync(
            GroqChatHistory history,
            string prompt,
            string? systemPrompt = null
        )
        {
            // Conta quantas vezes o usuário já enviou prompt
            var userTurns = history.Count(m => m.Role == "user");

            // 1) Se for a primeira mensagem e houver systemPrompt, injeta como "system"
            if (userTurns == 0 
                && !string.IsNullOrWhiteSpace(systemPrompt))
            {
                history.Add(new GroqMessage(systemPrompt) { Role = "system" });
            }

            // 2) Injeta o prompt do usuário
            history.Add(new GroqMessage(prompt) { Role = "user" });

            // 3) Chama o GroqClient diretamente
            var response = await _groq.GetChatCompletionsAsync(history);

            // 4) Retorna o conteúdo da primeira escolha
            return response.Choices.First().Message.Content;
        }
    }
}