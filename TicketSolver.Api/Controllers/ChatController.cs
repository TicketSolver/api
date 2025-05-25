using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Models.Chat;
using System.Security.Claims;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatService service) : ShellController
{
    [HttpPost("messages")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SendMessage([FromBody] SendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            // Verificar se o usuário tem acesso ao chat
            var hasAccess = await service.CanAccessChatAsync(request.TicketId, userId, userRole ?? "User", cancellationToken);
            if (!hasAccess)
            {
                return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString() ?? throw new InvalidOperationException());
            }
            
            request.SenderId = userId;
            request.SenderName = userName ?? "Usuário";
            request.SenderType = userRole switch
            {
                "1" => "Admin",
                "2" => "Technician", 
                "3" => "User",
                _ => "User"
            };

            var response = await service.SendMessageAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(response, "Mensagem enviada com sucesso!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro interno do servidor."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/history")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatHistory(int ticketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var hasAccess = await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken);
            if (!hasAccess)
            {
                return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString() ?? string.Empty);
            }

            var history = await service.GetChatHistoryAsync(ticketId, page, pageSize, cancellationToken);
            return Ok(ApiResponse.Ok(history));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter histórico do chat."));
        }
    }

    [HttpPost("messages/mark-read")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> MarkMessagesAsRead([FromBody] MarkAsReadRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var hasAccess = await service.CanAccessChatAsync(request.TicketId, userId, userType ?? "User", cancellationToken);
            if (!hasAccess)
            {
                return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString());
            }

            request.UserId = userId;
            request.UserType = userType ?? "User";
            
            await service.MarkMessagesAsReadAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok("", "Mensagens marcadas como lidas."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao marcar mensagens como lidas."));
        }
    }

    [HttpPost("tickets/{ticketId:int}/start")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> StartChat(int ticketId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var hasAccess = await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken);
            if (!hasAccess)
            {
                return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString());
            }

            var chat = await service.StartChatAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(chat, "Chat iniciado com sucesso!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao iniciar chat."));
        }
    }

    [HttpGet("unread")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatsWithUnreadMessages(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var chats = await service.GetChatsWithUnreadMessagesAsync(userId, userType ?? "User", cancellationToken);
            return Ok(ApiResponse.Ok(chats));
        }
        catch (Exception ex)
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
            var chats = await service.GetRecentChatsAsync(limit, cancellationToken);
            return Ok(ApiResponse.Ok(chats));
        }
        catch (Exception ex)
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
            var statistics = await service.GetChatStatisticsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(statistics));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter estatísticas do chat."));
        }
    }

    [HttpPost("search")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SearchMessages([FromBody] ChatSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            if (userType == "3" && request.TicketId.HasValue)
            {
                var hasAccess = await service.CanAccessChatAsync(request.TicketId.Value, userId, "User", cancellationToken);
                if (!hasAccess)
                {
                    return Forbid(ApiResponse.Fail("Acesso negado.").ToString());
                }
            }

            var messages = await service.SearchMessagesAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(messages));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao buscar mensagens."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/unread-count")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetUnreadMessageCount(int ticketId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var hasAccess = await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken);
            if (!hasAccess)
            {
                return Forbid(ApiResponse.Fail("Acesso negado.").ToString());
            }

            var count = await service.GetUnreadMessageCountAsync(ticketId, userId, userType ?? "User", cancellationToken);
            return Ok(ApiResponse.Ok(new { unreadCount = count }));
        }
        catch (Exception ex)
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
            var participants = await service.GetChatParticipantsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(participants));
        }
        catch (Exception ex)
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
            var response = await service.SendSystemMessageAsync(request, cancellationToken);
            return Ok(ApiResponse.Ok(response, "Mensagem do sistema enviada com sucesso!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao enviar mensagem do sistema."));
        }
    }

    [HttpGet("activity-summary")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetChatActivitySummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate == default || endDate == default)
            {
                return BadRequest(ApiResponse.Fail("Datas de início e fim são obrigatórias."));
            }

            var summary = await service.GetChatActivitySummaryAsync(startDate, endDate, cancellationToken);
            return Ok(ApiResponse.Ok(summary));
        }
        catch (Exception ex)
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
            await service.ArchiveChatAsync(ticketId, isArchived, cancellationToken);
            var message = isArchived ? "Chat arquivado com sucesso!" : "Chat desarquivado com sucesso!";
            return Ok(ApiResponse.Ok("", message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao arquivar/desarquivar chat."));
        }
    }

    [HttpGet("tickets/{ticketId:int}/info")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> GetChatInfo(int ticketId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);
        
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        try
        {
            var hasAccess = await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken);
            if (!hasAccess && userType == "3")
            {
                return Forbid(ApiResponse.Fail("Acesso negado.").ToString());
            }

            var info = await service.GetChatInfoAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(info));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter informações do chat."));
        }
    }
}
