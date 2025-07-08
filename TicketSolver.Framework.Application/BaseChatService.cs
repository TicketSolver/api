using System.Threading;
using System.Threading.Tasks;
using TicketSolver.Application.Models;
using TicketSolver.Application.Ports;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Framework.Application;

public class BaseChatService : IChatService
{
    private readonly IAiContextProvider _contextProvider;
    private readonly IAiProvider _aiProvider;

    public BaseChatService(
        IAiContextProvider contextProvider,
        IAiProvider aiProvider)
    {
        _contextProvider = contextProvider;
        _aiProvider = aiProvider;
    }

    public async Task<string> CreateTicketAsync(CancellationToken cancellationToken, TicketDTO ticketDto)
    {
        var tenant = new Tenant(ticketDto.Category.ToString());
        var context = await _contextProvider.GetAiContext(tenant, cancellationToken);

        var prompt = $"{context.SystemPrompt}\nTítulo: {ticketDto.Title}\nDescrição: {ticketDto.Description}";
        return await _aiProvider.GenerateTextAsync(prompt, cancellationToken);
    }
}
