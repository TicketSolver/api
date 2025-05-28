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

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly ITicketsRepository _ticketRepository;
    private readonly ILogger<ChatService> _logger; 
    private readonly IChatAiService   _chatAiService;

    public ChatService(
        IChatRepository chatRepository,
        ITicketsRepository ticketRepository,
        ILogger<ChatService> logger,
        IChatAiService chatAiService
        ITicketUsersRepository ticketUsersRepository)  
    {
        _chatRepository = chatRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
        _chatAiService    = chatAiService;
    }

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

        if (!await _ticketRepository.ExistsAsync(request.TicketId, cancellationToken))
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
            Timestamp     = DateTime.UtcNow,
            IsRead        = false
        };

        var updatedChat = await _chatRepository
            .AddMessageToChatAsync(request.TicketId, message, cancellationToken);

        _logger.LogInformation(
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
        var aiReplyText = await _chatAiService.AskAsync(
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
        await _chatRepository.AddMessageToChatAsync(request.TicketId, aiMessage, cancellationToken);
            _logger.LogInformation("Mensagem enviada para o ticket {TicketId} por {SenderName}", request.TicketId, request.SenderName);
        

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
        _logger.LogError(
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
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            
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
            _logger.LogError(ex, "Erro ao obter histórico do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task MarkMessagesAsReadAsync(MarkAsReadRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _chatRepository.MarkMessagesAsReadAsync(request.TicketId, request.UserId, request.UserType, cancellationToken);
            _logger.LogInformation("Mensagens marcadas como lidas para o usuário {UserId} no ticket {TicketId}", request.UserId, request.TicketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar mensagens como lidas para o usuário {UserId} no ticket {TicketId}", request.UserId, request.TicketId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatSummaryDto>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await _chatRepository.GetChatsWithUnreadMessagesAsync(userId, userType, cancellationToken);
            
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
            _logger.LogError(ex, "Erro ao obter chats com mensagens não lidas para o usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await _chatRepository.GetRecentChatsAsync(limit, cancellationToken);
            
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
            _logger.LogError(ex, "Erro ao obter chats recentes");
            throw;
        }
    }

    public async Task<ChatStatisticsDto> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _chatRepository.GetChatStatisticsAsync(ticketId, cancellationToken);
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
        
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
            _logger.LogError(ex, "Erro ao obter estatísticas do chat do ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<ChatResponseDto> StartChatAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _ticketRepository.ExistsAsync(ticketId, cancellationToken))
                throw new ArgumentException("Ticket não encontrado");

            var existingChat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
        
            if (existingChat == null)
            {
                _logger.LogInformation("Chat será criado quando a primeira mensagem for enviada para o ticket {TicketId}", ticketId);
            
                return new ChatResponseDto
                {
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
            _logger.LogError(ex, "Erro ao iniciar chat para o ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<bool> CanAccessChatAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Implementar lógica de permissão baseada no seu domínio
            // Por exemplo: verificar se o usuário é dono do ticket, técnico responsável, etc.
            
            var ticketCreatedById = await ticketRepository
                .GetById(ticketId)
                .Select(t => t.CreatedById)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (ticketCreatedById == null) return false;

            // Admins e técnicos podem acessar qualquer chat
            if (userType.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                userType.Equals("Technician", StringComparison.OrdinalIgnoreCase))
                return true;

            // Usuários só podem acessar seus próprios tickets
            if (userType.Equals("User", StringComparison.OrdinalIgnoreCase))
                return ticketCreatedById == userId ||
                       await ticketUsersRepository.IsUserAssignedToTicketAsync(cancellationToken, userId, ticketId);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar permissão de acesso ao chat do ticket {TicketId} para o usuário {UserId}", ticketId, userId);
            return false;
        }
    }

    public async Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(ChatSearchRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Esta implementação depende de você adicionar um método de busca no repository
            // Por enquanto, vou simular uma busca básica
            
            var chat = await _chatRepository.GetChatByTicketIdAsync(request.TicketId ?? 0, cancellationToken);
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
            _logger.LogError(ex, "Erro ao buscar mensagens");
            throw;
        }
    }

    public async Task<int> GetUnreadMessageCountAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null) return 0;

            return chat.Messages.Count(m => !m.IsRead && m.SenderId.ToString() != userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de mensagens não lidas para o ticket {TicketId}", ticketId);
            throw;
        }
    }

    public async Task<IEnumerable<ChatParticipantDto>> GetChatParticipantsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
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
            _logger.LogError(ex, "Erro ao obter participantes do chat do ticket {TicketId}", ticketId);
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

            var updatedChat = await _chatRepository.AddMessageToChatAsync(request.TicketId, message, cancellationToken);

            _logger.LogInformation("Mensagem do sistema enviada para o ticket {TicketId}", request.TicketId);

            return new ChatMessageResponseDto
            {
                Id = message.Id,
                TicketId = request.TicketId,
                SenderId = message.SenderId.ToString(),
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
            _logger.LogError(ex, "Erro ao enviar mensagem do sistema para o ticket {TicketId}", request.TicketId);
            throw;
        }
    }

    public async Task<ChatActivitySummaryDto> GetChatActivitySummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var chats = await _chatRepository.GetChatsByDateRangeAsync(startDate, endDate, cancellationToken);

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
            _logger.LogError(ex, "Erro ao obter resumo de atividade do chat");
            throw;
        }
    }

    public async Task ArchiveChatAsync(int ticketId, bool isArchived, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
            if (chat == null)
                throw new ArgumentException("Chat não encontrado");

            chat.IsArchived = isArchived;
            await _chatRepository.UpdateAsync(chat, cancellationToken);

            _logger.LogInformation("Chat do ticket {TicketId} {Action}", ticketId, isArchived ? "arquivado" : "desarquivado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao {Action} chat do ticket {TicketId}", isArchived ? "arquivar" : "desarquivar", ticketId);
            throw;
        }
    }

    public async Task<ChatInfoDto> GetChatInfoAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var chat = await _chatRepository.GetChatByTicketIdAsync(ticketId, cancellationToken);
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
            _logger.LogError(ex, "Erro ao obter informações do chat do ticket {TicketId}", ticketId);
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
