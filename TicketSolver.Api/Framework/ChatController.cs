using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Application.Models;
using TicketSolver.Application.Services.Tenant.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Framework.Application;
using TicketSolver.Framework.Domain;

namespace TicketSolver.Api.Framework;

[ApiController]
[Route("api/framework/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAiContextRepository _repo;
    private readonly ITenantsService    _tenantsService;

    public ChatController(IChatService chatService, IAiContextRepository repo,ITenantsService tenantsService)
    {
        _chatService = chatService;
        _repo = repo;
        _tenantsService  = tenantsService;
    }

    [HttpPost("start")]
    public async Task<ActionResult<string>> StartChatAsync(
        [FromBody] TicketDTO ticketDto,
        CancellationToken cancellationToken)
    {
        var result = await _chatService.CreateTicketAsync(cancellationToken, ticketDto);
        return Ok(result);
    }

    [HttpPost("context/{tenantKey:guid}")]
    public async Task<IActionResult> AddAiContextAsync(
        Guid tenantKey,
        [FromBody] string systemPrompt,
        CancellationToken cancellationToken)
    {
        // 1) Busca o tenant completo
        var tenant = await _tenantsService
            .GetTenantByKeyAsync(tenantKey, cancellationToken);

        if (tenant is null)
            return NotFound($"Tenant '{tenantKey}' não encontrado.");

        // 2) Cria o contexto de AI
        var aiContext = new AiContext(systemPrompt);

        // 3) Persiste vinculado à entidade carregada
        await _repo.AddAiContext(cancellationToken, aiContext, tenant);

        return Ok();
    }
}
