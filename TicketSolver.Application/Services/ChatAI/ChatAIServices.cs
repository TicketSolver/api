using GroqNet;
using GroqNet.ChatCompletions;
using TicketSolver.Application.Services.ChatAI.Interface;

namespace TicketSolver.Application.Services.ChatAI
{
    public class ChatService : IChatService
    {
        private readonly GroqClient _groq;

        public ChatService(GroqClient groqClient)
            => _groq = groqClient;

        public async Task<string> AskAsync(GroqChatHistory history, CancellationToken cancellationToken)
        {
            var response = await _groq.GetChatCompletionsAsync(history, cancellationToken: cancellationToken);
            return response.Choices[0].Message.Content;
        }
    }
}