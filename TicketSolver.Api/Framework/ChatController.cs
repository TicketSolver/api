using Microsoft.AspNetCore.Mvc;
using TicketSolver.Application.Models;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Framework;

[ApiController]
[Route("api/framework/chat")]
public class ChatController(IChatService chatService, IAiContextRepository repo) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<string>> StartChatAsync(
        [FromBody] TicketDTO ticketDto,
        CancellationToken cancellationToken)
    {
        var result = await chatService.CreateTicketAsync(cancellationToken, ticketDto);
        return Ok(result);
    }

    [HttpPost("context/{tenant}")]
    public async Task<IActionResult> AddAiContextAsync(
        string tenant,
        [FromBody] string systemPrompt,
        CancellationToken cancellationToken)
    {
        var context = new AiContext(systemPrompt);
        await repo.AddAiContext(cancellationToken, context, new Tenant(tenant));
        return Ok();
    }
}
