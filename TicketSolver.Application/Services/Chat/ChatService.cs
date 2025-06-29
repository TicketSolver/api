using GroqNet.ChatCompletions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketSolver.Application.Models;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Application.Services.ChatAI.Interface;
using TicketSolver.Domain.Enums;
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
    public async Task<ChatMessageResponseDto> SendDirectMessageAsync(
    SendMessageRequestDto request,
    CancellationToken cancellationToken = default)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Mensagem não pode estar vazia");

        if (!await ticketRepository.ExistsAsync(request.TicketId, cancellationToken))
            throw new ArgumentException("Ticket não encontrado");

        // Envia apenas a mensagem do usuário/técnico (sem IA)
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

        await chatRepository.AddMessageToChatAsync(request.TicketId, userMessage, cancellationToken);

        logger.LogInformation(
            "Mensagem direta enviada para o ticket {TicketId} por {SenderName}",
            request.TicketId, request.SenderName);

        return new ChatMessageResponseDto
        {
            Id            = userMessage.Id,
            TicketId      = request.TicketId,
            SenderId      = userMessage.SenderId,
            SenderType    = userMessage.SenderType,
            SenderName    = userMessage.SenderName,
            Text          = userMessage.text,
            MessageType   = userMessage.MessageType,
            AttachmentUrl = userMessage.AttachmentUrl,
            IsRead        = userMessage.IsRead,
            Timestamp     = userMessage.Timestamp
        };
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao enviar mensagem direta para o ticket {TicketId}", request.TicketId);
        throw;
    }
}

public async Task<bool> IsTicketAssignedToTechnicianAsync(int ticketId, CancellationToken cancellationToken = default)
{
    try
    {
        return await ticketUsersRepository.HasTechnicianAssignedAsync(ticketId, cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao verificar se ticket {TicketId} tem técnico atribuído", ticketId);
        return false;
    }
}

public async Task TransferToTechnicianAsync(int ticketId, string userId, CancellationToken cancellationToken = default)
{
    try
    {
        // 1. Verifica se já existe técnico atribuído
        if (await IsTicketAssignedToTechnicianAsync(ticketId, cancellationToken))
        {
            logger.LogInformation("Ticket {TicketId} já possui técnico atribuído", ticketId);
            return;
        }

        // 2. Busca técnico disponível
        var availableTechnicians = await GetAvailableTechniciansAsync(cancellationToken);
        if (!availableTechnicians.Any())
        {
            logger.LogWarning("Nenhum técnico disponível para o ticket {TicketId}", ticketId);
            
            // Envia mensagem do sistema informando que não há técnicos disponíveis
            await SendSystemMessageAsync(new SendSystemMessageRequestDto
            {
                TicketId = ticketId,
                Text = "Todos os técnicos estão ocupados no momento. Seu ticket foi adicionado à fila de atendimento.",
                MessageType = "text"
            }, cancellationToken);
            
            return;
        }

        // 3. Atribui ao primeiro técnico disponível
        var assignedTechnicianId = availableTechnicians.First().ToString();
        
        await ticketUsersRepository.AssignTechnicianToTicketAsync(
            ticketId, 
            assignedTechnicianId, 
            cancellationToken);

        // 4. Atualiza status do ticket
        await ticketRepository.UpdateStatusAsync(ticketId, eDefTicketStatus.InProgress, cancellationToken);

        // 5. Envia mensagem do sistema
        await SendSystemMessageAsync(new SendSystemMessageRequestDto
        {
            TicketId = ticketId,
            Text = "Seu ticket foi transferido para um técnico especializado. Em breve você receberá uma resposta personalizada.",
            MessageType = "text"
        }, cancellationToken);

        logger.LogInformation(
            "Ticket {TicketId} transferido para técnico {TechnicianId}", 
            ticketId, assignedTechnicianId);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao transferir ticket {TicketId} para técnico", ticketId);
        throw;
    }
}
public async Task<IEnumerable<TicketPendingResponseDto>> GetTicketsPendingTechnicianResponseAsync(CancellationToken cancellationToken = default)
{
    try
    {
        logger.LogInformation("Buscando tickets pendentes de resposta técnica");
        
        var tickets = await ticketRepository.GetTicketsWithUnreadUserMessagesAsync(cancellationToken);

        var ticketsEnumerable = tickets.ToList();
        logger.LogInformation($"Encontrados {ticketsEnumerable?.Count ?? 0} tickets");
        
        if (tickets == null || !ticketsEnumerable.Any())
        {
            logger.LogInformation("Nenhum ticket encontrado");
            return new List<TicketPendingResponseDto>();
        }

        var pendingTickets = new List<TicketPendingResponseDto>();

        foreach (var ticket in ticketsEnumerable)
        {
            try
            {
                if (ticket == null)
                {
                    logger.LogWarning("Ticket null encontrado, pulando...");
                    continue;
                }

                logger.LogInformation($"Processando ticket {ticket.Id}");

                // Verificar se existe chat para este ticket
                var chat = await chatRepository.GetChatByTicketIdAsync(ticket.Id, cancellationToken);
                
                if (chat == null)
                {
                    logger.LogWarning($"Chat não encontrado para ticket {ticket.Id}");
                    continue;
                }

                logger.LogInformation($"Chat encontrado para ticket {ticket.Id} - TotalMessages: {chat.TotalMessages}");

                // Verificar se há mensagens não lidas
                var messages = chat.Messages;
                if (messages == null)
                {
                    logger.LogWarning($"Mensagens null para ticket {ticket.Id}");
                    continue;
                }

                var lastUserMessage = messages
                    .Where(m => m?.SenderType == "User")
                    .OrderByDescending(m => m?.Timestamp)
                    .FirstOrDefault();

                if (lastUserMessage == null)
                {
                    logger.LogInformation($"Nenhuma mensagem de usuário encontrada para ticket {ticket.Id}");
                    continue;
                }

                // Verificar se há resposta técnica após a última mensagem do usuário
                var hasRecentTechResponse = messages
                    .Any(m => m?.SenderType != "User" && 
                             m?.SenderType != "assistant" && 
                             m?.Timestamp > lastUserMessage.Timestamp);

                if (hasRecentTechResponse)
                {
                    logger.LogInformation($"Ticket {ticket.Id} já tem resposta técnica recente");
                    continue;
                }

                // Buscar nome do técnico atribuído
                var technicianName = await ticketUsersRepository
                    .GetAssignedTechnicianNameAsync(ticket.Id, cancellationToken);

                pendingTickets.Add(new TicketPendingResponseDto
                {
                    TicketId = ticket.Id,
                    TicketTitle = ticket.Title ?? "Sem título",
                    CreatedBy = ticket.CreatedBy?.FullName ?? "Usuário desconhecido",
                    LastUserMessageAt = lastUserMessage.Timestamp,
                    LastUserMessage = lastUserMessage.text?.Substring(0, Math.Min(100, lastUserMessage.text.Length)) ?? "",
                    AssignedTechnician = technicianName ?? "Não atribuído",
                    Priority = GetPriorityText(ticket.DefTicketPriorityId),
                    CreatedAt = ticket.CreatedAt
                });

                logger.LogInformation($"Ticket {ticket.Id} adicionado à lista de pendentes");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Erro ao processar ticket {ticket?.Id ?? 0}");
                continue; // Continua com o próximo ticket
            }
        }

        logger.LogInformation($"Retornando {pendingTickets.Count} tickets pendentes");
        return pendingTickets;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao buscar tickets pendentes de resposta técnica");
        throw;
    }
}

private string GetPriorityText(int priorityId)
{
    return priorityId switch
    {
        1 => "Baixa",
        2 => "Normal",
        3 => "Alta",
        4 => "Crítica",
        _ => "Normal"
    };
}

private async Task<string> GetAssignedTechnicianNameAsync(int ticketId, CancellationToken cancellationToken)
{
    try
    {
        return await ticketUsersRepository.GetAssignedTechnicianNameAsync(ticketId, cancellationToken);
    }
    catch
    {
        return "Não atribuído";
    }
}

public async Task<IEnumerable<string>> GetAvailableTechniciansAsync(CancellationToken cancellationToken = default)
{
    try
    {
        return await ticketUsersRepository.GetAvailableTechniciansAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao buscar técnicos disponíveis");
        throw;
    }
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

        // 2) Prepara o histórico COMPLETO
        var allMessages = updatedChat.Messages
            .Where(m => m.SenderType != "System")
            .OrderBy(m => m.Timestamp)
            .ToList();

        var groqHistory = new GroqChatHistory();

        // 3) Adiciona system prompt apenas na primeira interação
        var userMessageCount = allMessages.Count(m => m.SenderType != "assistant");
        if (userMessageCount == 1)
        {
            var ticket = await ticketRepository.GetByIdAsync(request.TicketId);
            var systemPrompt = $"Você é um assistente técnico que precisa me ajudar a resolver o seguinte problema: {ticket?.Title}, {ticket?.Description}. Mantenha o contexto da conversa e referencie mensagens anteriores quando relevante.";
            groqHistory.Add(new GroqMessage(systemPrompt) { Role = "system" });
        }

        // 4) Adiciona TODAS as mensagens ao histórico
        foreach (var msg in allMessages)
        {
            if (string.IsNullOrWhiteSpace(msg.text)) continue;
            
            var role = msg.SenderType == "assistant" ? "assistant" : "user";
            groqHistory.Add(new GroqMessage(msg.text) { Role = role });
        }

        logger.LogInformation($"Histórico preparado com {groqHistory.Count} mensagens para ticket {request.TicketId}");

        // 5) Chama a IA com histórico completo
        var aiReplyText = await chatAiService.AskWithFullHistoryAsync(groqHistory);

        // 6) Persiste a resposta da IA
        var aiMessage = new Message
        {
            Id            = Guid.NewGuid().ToString(),
            SenderId      = "IA",
            SenderType    = "assistant",
            SenderName    = "IA",
            text          = aiReplyText,
            MessageType   = "text",
            AttachmentUrl = null,
            Timestamp     = DateTime.Now,
            IsRead        = false
        };

        await chatRepository.AddMessageToChatAsync(request.TicketId, aiMessage, cancellationToken);

        logger.LogInformation("IA respondeu para o ticket {TicketId}", request.TicketId);

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
        logger.LogError(ex, "Erro ao enviar mensagem para o ticket {TicketId}", request.TicketId);
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
                    CreatedAt = DateTime.Now,
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
        logger.LogInformation($"CanAccessChatAsync - TicketId: {ticketId}, UserId: {userId}, UserType: {userType}");
        
        var isAdmin      = userType == "1";
        var isTech       = userType == "2";
        var isRegular    = userType == "3";

        logger.LogInformation($"User types - Admin: {isAdmin}, Tech: {isTech}, Regular: {isRegular}");

        if (isAdmin || isTech)
        {
            logger.LogInformation("Access granted - Admin or Tech user");
            return true;
        }

        // usuário comum só se for dono ou estiver atribuído
        if (isRegular)
        {
            logger.LogInformation("Checking regular user access...");
            
            var createdBy = await ticketRepository.GetByIdAsync(ticketId);
            logger.LogInformation($"Ticket found: {createdBy != null}");
            
            if (createdBy == null) 
            {
                logger.LogWarning($"Ticket {ticketId} not found");
                return false;
            }

            logger.LogInformation($"Ticket CreatedById: '{createdBy.CreatedById}', Current UserId: '{userId}'");
            logger.LogInformation($"Are equal: {createdBy.CreatedById == userId}");

            if (createdBy.CreatedById == userId) 
            {
                logger.LogInformation("Access granted - User is ticket owner");
                return true;
            }

            // ou se está atribuído
            logger.LogInformation("Checking if user is assigned to ticket...");
            var isAssigned = await ticketUsersRepository
                .IsUserAssignedToTicketAsync(cancellationToken, userId, ticketId);
            
            logger.LogInformation($"User assigned: {isAssigned}");
            
            return isAssigned;
        }

        logger.LogWarning("Access denied - No valid user type");
        return false;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error in CanAccessChatAsync - TicketId: {ticketId}, UserId: {userId}");
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
                Timestamp = DateTime.Now,
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
                    CreatedAt = DateTime.Now,
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
