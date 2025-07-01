using GroqNet.ChatCompletions;
using System.Threading;
using GroqNet;
using System.Threading.Tasks;

namespace TicketSolver.Application.Services.ChatAI.Interface
{
    public interface IChatAiService
    {
        
        Task<string> AskAsync(GroqChatHistory history, string prompt, string? systemPrompt = null);
        Task<string> AskWithFullHistoryAsync(GroqChatHistory history); // NOVO
    }
}
