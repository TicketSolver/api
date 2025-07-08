using TicketSolver.Application.Models;
using TicketSolver.Application.Ports;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Framework.Application;

public class BaseChatService(
    IAiContextProvider contextProvider,
    IAiProvider aiProvider)
    : IChatService
{
    public async Task<string> CreateTicketAsync(CancellationToken cancellationToken, TicketDTO ticketDto)
    {
        var tenant = new Tenant(ticketDto.Category.ToString());
        var context = await contextProvider.GetAiContext(tenant);

        var prompt = $"{context.SystemPrompt}\nTítulo: {ticketDto.Title}\nDescrição: {ticketDto.Description}";
        return await aiProvider.GenerateTextAsync(prompt, cancellationToken);
    }
}
