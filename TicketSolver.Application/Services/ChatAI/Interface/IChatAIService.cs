using GroqNet.ChatCompletions;
using System.Threading;
using GroqNet;
using System.Threading.Tasks;

namespace TicketSolver.Application.Services.ChatAI.Interface
{
    public interface IChatAiService
    {
      
        /// Envia o histórico + prompt (e opcional systemPrompt) para o Groq e devolve a resposta.
        Task<string> AskAsync(
            GroqChatHistory history,
            string prompt,
            string? systemPrompt = null
        );
    }
}
