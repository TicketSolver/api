using GroqNet.ChatCompletions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI.Interface;
using TicketSolver.Domain.Models.Chat;
using TicketSolver.Domain.Persistence.Tables.Chat;
using TicketSolver.Domain.Repositories.Chat;

using TicketSolver.Domain.Repositories.Ticket;

namespace TicketSolver.Application.Services.Chat;

public class ChatService(
    IChatRepository chatRepository,
    ITicketsRepository ticketRepository,
    ILogger<ChatService> logger,
    IChatAiService chatAiService,
    ITicketUsersRepository ticketUsersRepository)
    : IChatService
{
    public async Task<ChatMessageResponseDto> SendMessageAsync(
    SendMessageRequestDto request,
    CancellationToken cancellationToken = default
)
{
    try
    {
        // Validações
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Mensagem não pode estar vazia");

        if (!await ticketRepository.ExistsAsync(request.TicketId, cancellationToken))
            throw new ArgumentException("Ticket não encontrado");

        // 1) Persiste a mensagem do usuário
        var userMessage = new Message
        {
            Id            = Guid.NewGuid().ToString(),
            SenderId      = request.SenderId,
            SenderType    = request.SenderType,
            SenderName    = request.SenderName,
            text          = request.Text,
            MessageType   = request.MessageType,
            AttachmentUrl = request.AttachmentUrl,
            Timestamp     = DateTime.Now,
            IsRead        = false
        };

        var updatedChat = await chatRepository
            .AddMessageToChatAsync(request.TicketId, userMessage, cancellationToken);

        logger.LogInformation(
            "Mensagem enviada para o ticket {TicketId} por {SenderName}",
            request.TicketId, request.SenderName);

        // 2) Prepara o histórico para a IA
        var groqHistory = new GroqChatHistory(
            updatedChat.Messages
                .Select(m => new GroqMessage(m.text)
                {
                    Role = m.SenderType == "assistant" ? "assistant" : "user"
                })
                .ToList()
        );

        // 3) Chama a IA
        var aiReplyText = await chatAiService.AskAsync(
            groqHistory,
            request.Text,
            null // ou use request.SystemPrompt se existir
        );

        // 4) Persiste a resposta da IA
        var aiMessage = new Message
        {
            Id            = Guid.NewGuid().ToString(),
            SenderId      = "IA",
            SenderType    = "assistant",
            SenderName    = "IA",
            text          = aiReplyText,
            MessageType   = "text",
            AttachmentUrl = null,
            Timestamp     = DateTime.UtcNow,
            IsRead        = false
        };
        await chatRepository.AddMessageToChatAsync(request.TicketId, aiMessage, cancellationToken);
            logger.LogInformation("Mensagem enviada para o ticket {TicketId} por {SenderName}", request.TicketId, request.SenderName);
        

        // 5) Retorna a DTO da resposta da IA
        return new ChatMessageResponseDto
        {
            Id            = aiMessage.Id,
            TicketId      = request.TicketId,
            SenderId      = aiMessage.SenderId,
            SenderType    = aiMessage.SenderType,
            SenderName    = aiMessage.SenderName,
            Text          = aiMessage.text,
            MessageType   = aiMessage.MessageType,
            AttachmentUrl = aiMessage.AttachmentUrl,
            IsRead        = aiMessage.IsRead,
            Timestamp     = aiMessage.Timestamp
        };
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Erro ao enviar mensagem para o ticket {TicketId}",
            request.TicketId);
        throw;
    }
}


    public async Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        return await GetChatHistoryAsync(ticketId, 1, int.MaxValue, cancellationToken);
    }

    public async Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            
            if (chat == null)
            {
                return new ChatHistoryResponseDto
                {
                    TicketId = ticketId,
                    Messages = [],
                    TotalMessages = 0,
                    Page = page,
                    PageSize = pageSize,
                    HasNextPage = false
                };
            }

            var totalMessages = chat.Messages.Count;
            var skip = (page - 1) * pageSize;
            var paginatedMessages = chat.Messages
                .OrderBy(m => m.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new ChatHistoryResponseDto
            {
                TicketId = ticketId,
                Messages = paginatedMessages,
                TotalMessages = totalMessages,
                Page = page,
                PageSize = pageSize,
                HasNextPage = skip + pageSize < totalMessages,
                LastMessageAt = chat.Messages.LastOrDefault()?.Timestamp
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter histórico do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task MarkMessagesAsReadAsync(MarkAsReadRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            await chatRepository.MarkMessagesAsReadAsync(request.TicketId, request.UserId, request.UserType, cancellationToken);
            logger.LogInformation("Mensagens marcadas como lidas para o usuário {UserId} no ticket {TicketId}", request.UserId, request.TicketId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao marcar mensagens como lidas para o usuário {UserId} no ticket {TicketId}", request.UserId, request.TicketId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatSummaryDto>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await chatRepository.GetChatsWithUnreadMessagesAsync(userId, userType, cancellationToken);
            
            return chats.Select(chat => new ChatSummaryDto
            {
                TicketId = chat.TicketId,
                TicketTitle = $"Ticket #{chat.TicketId}", // Você pode buscar o título real do ticket
                TotalMessages = chat.Messages.Count,
                UnreadMessages = chat.Messages.Count(m => !m.IsRead),
                LastMessageAt = chat.Messages.LastOrDefault()?.Timestamp,
                LastMessageText = chat.Messages.LastOrDefault()?.text ?? "",
                LastSenderName = chat.Messages.LastOrDefault()?.SenderName ?? "",
                LastSenderType = chat.Messages.LastOrDefault()?.SenderType ?? ""
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter chats com mensagens não lidas para o usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await chatRepository.GetRecentChatsAsync(limit, cancellationToken);
            
            return chats.Select(chat => new ChatSummaryDto
            {
                TicketId = chat.TicketId,
                TicketTitle = $"Ticket #{chat.TicketId}",
                TotalMessages = chat.Messages.Count,
                UnreadMessages = chat.Messages.Count(m => !m.IsRead),
                LastMessageAt = chat.Messages.LastOrDefault()?.Timestamp,
                LastMessageText = chat.Messages.LastOrDefault()?.text ?? "",
                LastSenderName = chat.Messages.LastOrDefault()?.SenderName ?? "",
                LastSenderType = chat.Messages.LastOrDefault()?.SenderType ?? ""
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter chats recentes");
            throw;
        }
    }

    public async Task<ChatStatisticsDto> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await chatRepository.GetChatStatisticsAsync(ticketId, cancellationToken);
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
        
            // Calcular estatísticas adicionais se o chat existir
            var totalParticipants = 0;
            var messagesByType = new Dictionary<string, int>();
            var messagesBySender = new Dictionary<string, int>();
        
            if (chat != null)
            {
                totalParticipants = chat.Messages
                    .GroupBy(m => new { m.SenderId, m.SenderType })
                    .Count();
                
                messagesByType = chat.Messages
                    .GroupBy(m => m.MessageType)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                messagesBySender = chat.Messages
                    .GroupBy(m => m.SenderName)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
        
            return new ChatStatisticsDto
            {
                TicketId = statistics.TicketId,
                TotalMessages = statistics.TotalMessages,
                UnreadMessages = statistics.UnreadMessages,
                LastMessageAt = statistics.LastMessageAt,
                CreatedAt = statistics.CreatedAt,
                LastSenderType = statistics.LastSenderType,
                LastSenderName = statistics.LastSenderName,
                TotalParticipants = totalParticipants,
                MessagesByType = messagesByType,
                MessagesBySender = messagesBySender
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter estatísticas do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<ChatResponseDto> StartChatAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ticketRepository.ExistsAsync(ticketId, cancellationToken))
                throw new ArgumentException("Ticket não encontrado");

            var existingChat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
        
            if (existingChat == null)
            {
                var ticket = await ticketRepository.GetById(ticketId)
                    .Select(t => new { t.Title, t.Description })
                    .FirstOrDefaultAsync(cancellationToken);

                var systemPrompt =
                    $"Você é um assistente técnico que precisa me ajudar a resolver o seguinte problema: {ticket.Title}, {ticket.Description}";
                logger.LogInformation("Chat será criado quando a primeira mensagem for enviada para o ticket {TicketId}", ticketId);
            
                return new ChatResponseDto
                {
                    SystemPrompt = systemPrompt,
                    TicketId = ticketId,
                    IsNewChat = true,
                    CreatedAt = DateTime.UtcNow,
                    TotalMessages = 0
                };
            }
        
            return new ChatResponseDto
            {
                TicketId = ticketId,
                IsNewChat = false,
                CreatedAt = existingChat.CreatedAt,
                TotalMessages = existingChat.Messages.Count
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao iniciar chat para o ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<bool> CanAccessChatAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAdmin      = userType == "1";
            var isTech       = userType == "2";
            var isRegular    = userType == "3";

            if (isAdmin || isTech)
                return true;

            // usuário comum só se for dono ou estiver atribuído
            if (isRegular)
            {
                var createdBy = await ticketRepository
                    .GetByIdAsync(ticketId);
                if (createdBy == null) 
                    return false;

                if (createdBy.CreatedById == userId) 
                    return true;

                // ou se está atribuído
                return await ticketUsersRepository
                    .IsUserAssignedToTicketAsync(cancellationToken, userId, ticketId);
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "…");
            return false;
        }
    }


    public async Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(ChatSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Esta implementação depende de você adicionar um método de busca no repository
            // Por enquanto, vou simular uma busca básica
            
            var chat = await chatRepository.GetChatByTicketIdAsync(request.TicketId ?? 0, cancellationToken);
            if (chat == null) return [];

            var messages = chat.Messages.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                messages = messages.Where(m => m.text.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(request.SenderType))
                messages = messages.Where(m => m.SenderType == request.SenderType);

            if (!string.IsNullOrWhiteSpace(request.SenderId))
                messages = messages.Where(m => m.SenderId.ToString() == request.SenderId);

            if (request.StartDate.HasValue)
                messages = messages.Where(m => m.Timestamp >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                messages = messages.Where(m => m.Timestamp <= request.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(request.MessageType))
                messages = messages.Where(m => m.MessageType == request.MessageType);

            var skip = (request.Page - 1) * request.PageSize;
            return messages
                .OrderByDescending(m => m.Timestamp)
                .Skip(skip)
                .Take(request.PageSize)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar mensagens");
            throw;
        }
    }

    public async Task<int> GetUnreadMessageCountAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null) return 0;

            return chat.Messages.Count(m => !m.IsRead && m.SenderId.ToString() != userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter contagem de mensagens não lidas para o ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatParticipantDto>> GetChatParticipantsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null) return [];

            var participants = chat.Messages
                .GroupBy(m => new { m.SenderId, m.SenderName, m.SenderType })
                .Select(g => new ChatParticipantDto
                {
                    UserId = g.Key.SenderId.ToString(),
                    UserName = g.Key.SenderName,
                    UserType = g.Key.SenderType,
                    LastSeenAt = g.Max(m => m.Timestamp),
                    MessageCount = g.Count()
                })
                .OrderBy(p => p.UserName);

            return participants;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter participantes do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<ChatMessageResponseDto> SendSystemMessageAsync(SendSystemMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = Guid.NewGuid().ToString(), 
                SenderType = "System",
                SenderName = "Sistema",
                text = request.Text,
                MessageType = request.MessageType,
                AttachmentUrl = request.AttachmentUrl,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            var updatedChat = await chatRepository.AddMessageToChatAsync(request.TicketId, message, cancellationToken);

            logger.LogInformation("Mensagem do sistema enviada para o ticket {TicketId}", request.TicketId);

            return new ChatMessageResponseDto
            {
                Id = message.Id,
                TicketId = request.TicketId,
                SenderId = message.SenderId,
                SenderType = message.SenderType,
                SenderName = message.SenderName,
                Text = message.text,
                MessageType = message.MessageType,
                AttachmentUrl = message.AttachmentUrl,
                IsRead = message.IsRead,
                Timestamp = message.Timestamp
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao enviar mensagem do sistema para o ticket {TicketId}", request.TicketId);
            throw;
        }
    }

    public async Task<ChatActivitySummaryDto> GetChatActivitySummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await chatRepository.GetChatsByDateRangeAsync(startDate, endDate, cancellationToken);

            var totalChats = chats.Count();
            var totalMessages = chats.Sum(c => c.Messages.Count);
            var activeChats = chats.Count(c => c.Messages.Any(m => m.Timestamp >= startDate && m.Timestamp <= endDate));

            var messagesByDay = chats
                .SelectMany(c => c.Messages)
                .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
                .GroupBy(m => m.Timestamp.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Count());

            var messagesBySenderType = chats
                .SelectMany(c => c.Messages)
                .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
                .GroupBy(m => m.SenderType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new ChatActivitySummaryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalChats = totalChats,
                TotalMessages = totalMessages,
                ActiveChats = activeChats,
                MessagesByDay = messagesByDay,
                MessagesBySenderType = messagesBySenderType
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter resumo de atividade do chat");
            throw;
        }
    }

    public async Task ArchiveChatAsync(int ticketId, bool isArchived, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null)
                throw new ArgumentException("Chat não encontrado");

            chat.IsArchived = isArchived;
            await chatRepository.UpdateAsync(chat, cancellationToken);

            logger.LogInformation("Chat do ticket {TicketId} {Action}", ticketId, isArchived ? "arquivado" : "desarquivado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao {Action} chat do ticket {TicketId}", isArchived ? "arquivar" : "desarquivar", ticketId);
            throw;
        }
    }

    public async Task<ChatInfoDto> GetChatInfoAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null)
            {
                return new ChatInfoDto
                {
                    TicketId = ticketId,
                    TotalMessages = 0,
                    CreatedAt = DateTime.UtcNow,
                    HasUnreadMessages = false,
                    Participants = []
                };
            }

            var participants = await GetChatParticipantsAsync(ticketId, cancellationToken);

            return new ChatInfoDto
            {
                TicketId = ticketId,
                TotalMessages = chat.Messages.Count,
                LastMessageAt = chat.Messages.LastOrDefault()?.Timestamp,
                CreatedAt = chat.CreatedAt,
                HasUnreadMessages = chat.Messages.Any(m => !m.IsRead),
                Participants = participants.ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter informações do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    private static ChatMessageDto MapToDto(Message message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId.ToString(),
            SenderType = message.SenderType,
            SenderName = message.SenderName,
            Message = message.text,
            MessageType = message.MessageType,
            AttachmentUrl = message.AttachmentUrl,
            IsRead = message.IsRead,
            Timestamp = message.Timestamp
        };
    }
}
