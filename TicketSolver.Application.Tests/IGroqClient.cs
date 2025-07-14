using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroqNet.ChatCompletions;

namespace TicketSolver.Application.Tests;

public interface IGroqClient
{
    Task<GroqChatCompletions> GetChatCompletionsAsync(IList<GroqMessage> messages, GroqChatCompletionOptions? options = null, CancellationToken cancellationToken = default);
}
