using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Application.Models;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Framework;

[ApiController]
[Route("api/framework/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAiContextRepository _repo;

    public ChatController(IChatService chatService, IAiContextRepository repo)
    {
        _chatService = chatService;
        _repo = repo;
    }

    [HttpPost("start")]
    public async Task<ActionResult<string>> StartChatAsync(
        [FromBody] TicketDTO ticketDto,
        CancellationToken cancellationToken)
    {
        var result = await _chatService.CreateTicketAsync(cancellationToken, ticketDto);
        return Ok(result);
    }

    [HttpPost("context/{tenant}")]
    public async Task<IActionResult> AddAiContextAsync(
        string tenant,
        [FromBody] string systemPrompt,
        CancellationToken cancellationToken)
    {
        var context = new AiContext(systemPrompt);
        await _repo.AddAiContext(cancellationToken, context, new Tenant(tenant));
        return Ok();
    }
}
