using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GroqNet;
using System.Security.Claims;
using GroqNet.ChatCompletions;
using Microsoft.AspNetCore.Http.HttpResults;
using TicketSolver.Api.Models;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI.Interface;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Enums;
using TicketSolver.Infra.EntityFramework.Persistence;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ShellController
{
    private readonly IChatService   _chatService;
    private readonly IChatAiService _chatAiService;
    private readonly EfContext      _db;

    public ChatController(
        IChatService service,
        IChatAiService chatAiService,
        EfContext db
    ) : base()
    {
        _chatService   = service;
        _chatAiService = chatAiService;
        _db            = db;
    }

    [HttpPost("messages")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SendMessage([
        FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        // Conta quantas respostas da IA já existem
        var history = await _chatService.GetChatHistoryAsync(
            request.TicketId,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken
        );
        var aiCount = history.Messages.Count(m => m.SenderType == "assistant");
        if (aiCount >= 5)
        {
            // Persiste relacionamento em TicketUsers
            var ticketUser = new TicketUsers
            {
                UserId              = userId,
                TicketId            = request.TicketId,
                DefTicketUserRoleId = (short)eDefTicketUserRoles.Responder,
                AddedAt             = DateTime.UtcNow
            };
            _db.TicketUsers.Add(ticketUser);
            await _db.SaveChangesAsync(cancellationToken);

            return BadRequest(ApiResponse.Fail(
                "Mensagens excedidas. Entrando em contato com o técnico."
            ));
        }

        if (!await _chatService.CanAccessChatAsync(request.TicketId, userId, userRole ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        request.SenderId   = userId;
        request.SenderName = userName ?? "Usuário";
        request.SenderType = userRole switch
        {
            "1" => "Admin",
            "2" => "Technician",
            "3" => "User",
            _   => "User"
        };

        try
        {
            var response = await _chatService.SendMessageAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(response, "Mensagem enviada com sucesso!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro interno do servidor."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/history")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatHistory(
        int ticketId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default
    )
    {
        var userId = AuthenticatedUser.UserId;
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await _chatService.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        try
        {
            var history = await _chatService.GetChatHistoryAsync(ticketId, page, pageSize, cancellationToken);
            return Ok(ApiResponse.Ok(history));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter histórico do chat."));
        }
    }

    [HttpPost("messages/mark-read")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> MarkMessagesAsRead(
        [FromBody] MarkAsReadRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await _chatService.CanAccessChatAsync(request.TicketId, userId, userType ?? "User", cancellationToken))
            return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString());

        request.UserId   = userId;
        request.UserType = userType ?? "User";

        try
        {
            await _chatService.MarkMessagesAsReadAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok("", "Mensagens marcadas como lidas."));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao marcar mensagens como lidas."));
        }
    }

    [HttpPost("tickets/{ticketId:int}/start")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> StartChat(
        int ticketId,
        CancellationToken cancellationToken = default
    )
    {
        var userId = AuthenticatedUser.UserId;
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await _chatService.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        try
        {
            var chatInfo     = await _chatService.StartChatAsync(ticketId, cancellationToken);
            var history      = new GroqChatHistory();
            var initialReply = await _chatAiService.AskAsync(history, prompt: "", systemPrompt: null);

            var systemMsg = await _chatService.SendSystemMessageAsync(
                new SendSystemMessageRequestDto
                {
                    TicketId      = ticketId,
                    Text          = initialReply,
                    MessageType   = "text",
                    AttachmentUrl = null
                },
                cancellationToken
            );

            return Ok(new { data = new { chatInfo, initialMessage = systemMsg } });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao iniciar chat."));
        }
    }

    [HttpGet("unread")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatsWithUnreadMessages(CancellationToken cancellationToken = default)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        try
        {
            var chats = await _chatService.GetChatsWithUnreadMessagesAsync(userId, userType ?? "User", cancellationToken);
            return Ok(ApiResponse.Ok(chats));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter chats com mensagens não lidas."));
        }
    }

    [HttpGet("recent")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetRecentChats([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await _chatService.GetRecentChatsAsync(limit, cancellationToken);
            return Ok(ApiResponse.Ok(chats));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter chats recentes."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/statistics")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetChatStatistics(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _chatService.GetChatStatisticsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(statistics));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter estatísticas do chat."));
        }
    }

    [HttpPost("search")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SearchMessages([FromBody] ChatSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (userType == "3" && request.TicketId.HasValue)
        {
            if (!await _chatService.CanAccessChatAsync(request.TicketId.Value, userId, "User", cancellationToken))
                return Forbid(ApiResponse.Fail("Acesso negado.").ToString());
        }

        try
        {
            var messages = await _chatService.SearchMessagesAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(messages));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao buscar mensagens."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/unread-count")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetUnreadMessageCount(int ticketId, CancellationToken cancellationToken = default)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await _chatService.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return Forbid(ApiResponse.Fail("Acesso negado.").ToString());

        try
        {
            var count = await _chatService.GetUnreadMessageCountAsync(ticketId, userId, userType ?? "User", cancellationToken);
            return Ok(ApiResponse.Ok(new { unreadCount = count }));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter contagem de mensagens não lidas."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/participants")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetChatParticipants(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var participants = await _chatService.GetChatParticipantsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(participants));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter participantes do chat."));
        }
    }

    [HttpPost("system-message")]
    [Authorize(Roles = "1")]
    public async Task<ActionResult> SendSystemMessage([FromBody] SendSystemMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _chatService.SendSystemMessageAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(response, "Mensagem do sistema enviada com sucesso!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao enviar mensagem do sistema."));
        }
    }

    [HttpGet("activity-summary")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetChatActivitySummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (startDate == default || endDate == default)
            return BadRequest(ApiResponse.Fail("Datas de início e fim são obrigatórias."));

        try
        {
            var summary = await _chatService.GetChatActivitySummaryAsync(startDate, endDate, cancellationToken);
            return Ok(ApiResponse.Ok(summary));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter resumo de atividade."));
        }
    }

    [HttpPut("tickets/{ticketId:int}/archive")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> ArchiveChat(int ticketId, [FromBody] bool isArchived = true, CancellationToken cancellationToken = default)
    {
        try
        {
            await _chatService.ArchiveChatAsync(ticketId, isArchived, cancellationToken);
            var message = isArchived ? "Chat arquivado com sucesso!" : "Chat desarquivado com sucesso!";
            return Ok(ApiResponse.Ok("", message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao arquivar/desarquivar chat."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/info")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatInfo(int ticketId, CancellationToken cancellationToken = default)
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await _chatService.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken) && userType == "3")
            return Forbid(ApiResponse.Fail("Acesso negado.").ToString());

        try
        {
            var info = await _chatService.GetChatInfoAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(info));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter informações do chat."));
        }
    }
}
