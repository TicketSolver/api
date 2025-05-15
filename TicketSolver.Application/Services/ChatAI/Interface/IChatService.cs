using GroqNet.ChatCompletions;
using System.Threading;

namespace TicketSolver.Application.Services.ChatAI.Interface
{
    public interface IChatService
    {
        Task<string> AskAsync(GroqChatHistory history, CancellationToken cancellationToken);
    }
}