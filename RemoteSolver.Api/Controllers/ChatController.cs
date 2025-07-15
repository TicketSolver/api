using System.Security.Claims;
using GroqNet.ChatCompletions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteSolver.Api.Models;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI.Interface;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts;

namespace RemoteSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IChatService service,
    IChatAiService chatAiService,
    EfContext db)
    : ShellController
{
    [HttpPost("messages")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SendMessage([
            FromBody]
        SendMessageRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await service.CanAccessChatAsync(request.TicketId, userId, userRole ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        request.SenderId = userId;
        request.SenderName = userName ?? "Usuário";
        request.SenderType = userRole switch
        {
            "1" => "Admin",
            "2" => "Technician",
            _ => "User"
        };

        try
        {
            var chatInfo = await service.GetChatInfoAsync(request.TicketId, cancellationToken);
            var isTransferredToTechnician =
                await service.IsTicketAssignedToTechnicianAsync(request.TicketId, cancellationToken);
            if (userRole == "3" && isTransferredToTechnician)
            {
                var response = await service.SendDirectMessageAsync(request, cancellationToken);
                return Ok(ApiResponse.Ok(response, "Mensagem enviada para o técnico!"));
            }

            if (userRole == "1" || userRole == "2")
            {
                var response = await service.SendDirectMessageAsync(request, cancellationToken);
                return Ok(ApiResponse.Ok(response, "Mensagem enviada com sucesso!"));
            }

            if (userRole == "3")
            {
                var history = await service.GetChatHistoryAsync(
                    request.TicketId,
                    page: 1,
                    pageSize: int.MaxValue,
                    cancellationToken
                );

                var aiMessageCount = history.Messages.Count(m => m.SenderType == "assistant");
                if (aiMessageCount >= 5)
                {
                    await service.TransferToTechnicianAsync(request.TicketId, userId, cancellationToken);

                    var directResponse = await service.SendDirectMessageAsync(request, cancellationToken);

                    return Ok(ApiResponse.Ok(directResponse,
                        "Limite de mensagens da IA atingido. Seu ticket foi transferido para um técnico que responderá em breve."));
                }

                var aiResponse = await service.SendMessageAsync(request, cancellationToken);
                return Ok(ApiResponse.Ok(aiResponse, "Mensagem enviada com sucesso!"));
            }

            return BadRequest(ApiResponse.Fail("Tipo de usuário inválido."));
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

    [HttpGet("pending-tickets")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetPendingTickets(CancellationToken cancellationToken = default)
    {
        try
        {
            var pendingTickets = await service.GetTicketsPendingTechnicianResponseAsync(cancellationToken);
            return Ok(ApiResponse.Ok(pendingTickets));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter tickets pendentes."));
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

        if (!await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        try
        {
            var history = await service.GetChatHistoryAsync(ticketId, page, pageSize, cancellationToken);
            return Ok(ApiResponse.Ok(history));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter histórico do chat."));
        }
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public ActionResult Test()
    {
        return Ok(new
        {
            message = "ChatController funcionando!",
            timestamp = DateTime.Now,
            controllerName = nameof(ChatController)
        });
    }

    [HttpPost("test-auth")]
    [Authorize(Roles = "1,2,3")]
    public ActionResult TestAuth()
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        var userName = User.FindFirstValue(ClaimTypes.Name);

        return Ok(new
        {
            message = "Autenticação funcionando!",
            AuthenticatedUser.UserId,
            userRole,
            userName,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpGet("debug/ticket/{ticketId:int}")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> DebugTicketAccess(int ticketId)
    {
        var userId = AuthenticatedUser.UserId;
        var userType = User.FindFirstValue(ClaimTypes.Role);

        try
        {
            // Verificar se o ticket existe
            var ticketExists = await db.Tickets.AnyAsync(t => t.Id == ticketId);

            // Buscar o ticket
            var ticket = await db.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.CreatedById,
                    CreatedByName = t.CreatedBy.FullName
                })
                .FirstOrDefaultAsync();

            // Verificar se está atribuído
            var isAssigned = await db.TicketUsers
                .AnyAsync(tu => tu.TicketId == ticketId && tu.UserId == userId);

            return Ok(new
            {
                ticketId,
                userId,
                userType,
                ticketExists,
                ticket,
                isOwner = ticket?.CreatedById == userId,
                isAssigned,
                debugInfo = new
                {
                    authUserId = AuthenticatedUser.UserId,
                    authIsAuthenticated = AuthenticatedUser.IsAuthenticated,
                    claims = User.Claims.Select(c => new { c.Type, c.Value })
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                error = ex.Message,
                userId,
                userType,
                ticketId
            });
        }
    }
    [HttpGet("debug/chat-history/{ticketId:int}")]
    [Authorize]
    public async Task<ActionResult> DebugChatHistory(int ticketId)
    {
        try
        {
            var chat = await db.Chats
                .Where(c => c.TicketId == ticketId)
                .FirstOrDefaultAsync();

            if (chat == null)
                return Ok(new { message = "Chat não encontrado" });

            return Ok(new {
                chatId = chat.Id,
                ticketId = chat.TicketId,
                totalMessages = chat.TotalMessages,
                chatHistoryRaw = chat.ChatHistory,
                messagesCount = chat.Messages?.Count ?? 0,
                messages = chat.Messages?.Select(m => new {
                    m.Id,
                    m.SenderType,
                    m.SenderName,
                    Text = m.text?.Substring(0, Math.Min(100, m.text?.Length ?? 0)),
                    m.Timestamp
                })
            });
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("messages/mark-read")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> MarkMessagesAsRead(
        [FromBody] MarkAsReadRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await service.CanAccessChatAsync(request.TicketId, userId, userType ?? "User", cancellationToken))
            return Forbid(ApiResponse.Fail("Acesso negado ao chat deste ticket.").ToString());

        request.UserId = userId;
        request.UserType = userType ?? "User";

        try
        {
            await service.MarkMessagesAsReadAsync(request, cancellationToken);
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

        if (!await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return BadRequest((ApiResponse.Fail("Acesso negado ao chat deste ticket.")));

        try
        {
            var chatInfo = await service.StartChatAsync(ticketId, cancellationToken);
            if (chatInfo.SystemPrompt == "") return Ok(ApiResponse.Ok(null));
            var history = new GroqChatHistory();
            var initialReply = await chatAiService.AskAsync(history, prompt: "Soluciona meu problema",
                systemPrompt: chatInfo.SystemPrompt);

            var systemMsg = await service.SendSystemMessageAsync(
                new SendSystemMessageRequestDto
                {
                    TicketId = ticketId,
                    Text = initialReply,
                    MessageType = "text",
                    AttachmentUrl = null
                },
                cancellationToken
            );
            var a = new { data = new { chatInfo, initialMessage = systemMsg.Text } };
            Console.WriteLine(a);
            return Ok(a);
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        try
        {
            var chats = await service.GetChatsWithUnreadMessagesAsync(userId, userType ?? "User",
                cancellationToken);
            return Ok(ApiResponse.Ok(chats));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter chats com mensagens não lidas."));
        }
    }

    [HttpGet("recent")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetRecentChats([FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await service.GetRecentChatsAsync(limit, cancellationToken);
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
            var statistics = await service.GetChatStatisticsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(statistics));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter estatísticas do chat."));
        }
    }

    [HttpPost("search")]
    [Authorize(Roles = "1,2,3")]
    public async Task<ActionResult> SearchMessages([FromBody] ChatSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (userType == "3" && request.TicketId.HasValue)
        {
            if (!await service.CanAccessChatAsync(request.TicketId.Value, userId, "User", cancellationToken))
                return Forbid(ApiResponse.Fail("Acesso negado.").ToString());
        }

        try
        {
            var messages = await service.SearchMessagesAsync(request, cancellationToken);
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userType = User.FindFirstValue(ClaimTypes.Role);

        if (userId is null)
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken))
            return Forbid(ApiResponse.Fail("Acesso negado.").ToString());

        try
        {
            var count = await service.GetUnreadMessageCountAsync(ticketId, userId, userType ?? "User",
                cancellationToken);
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
            var participants = await service.GetChatParticipantsAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(participants));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter participantes do chat."));
        }
    }

    [HttpPost("system-message")]
    [Authorize(Roles = "1")]
    public async Task<ActionResult> SendSystemMessage([FromBody] SendSystemMessageRequestDto request,
        CancellationToken cancellationToken = default)
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
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao enviar mensagem do sistema."));
        }
    }

    [HttpGet("activity-summary")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> GetChatActivitySummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (startDate == default || endDate == default)
            return BadRequest(ApiResponse.Fail("Datas de início e fim são obrigatórias."));

        try
        {
            var summary = await service.GetChatActivitySummaryAsync(startDate, endDate, cancellationToken);
            return Ok(ApiResponse.Ok(summary));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter resumo de atividade."));
        }
    }

    [HttpPut("tickets/{ticketId:int}/archive")]
    [Authorize(Roles = "1,2")]
    public async Task<ActionResult> ArchiveChat(int ticketId, [FromBody] bool isArchived = true,
        CancellationToken cancellationToken = default)
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
        catch
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
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));

        if (!await service.CanAccessChatAsync(ticketId, userId, userType ?? "User", cancellationToken) &&
            userType == "3")
            return Forbid(ApiResponse.Fail("Acesso negado.").ToString());

        try
        {
            var info = await service.GetChatInfoAsync(ticketId, cancellationToken);
            return Ok(ApiResponse.Ok(info));
        }
        catch
        {
            return StatusCode(500, ApiResponse.Fail("Erro ao obter informações do chat."));
        }
    }
}
