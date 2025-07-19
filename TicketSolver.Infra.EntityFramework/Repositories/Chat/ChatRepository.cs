using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Models.Chat;
using TicketSolver.Domain.Persistence.Tables.Chat;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Repositories.Chat;
using TicketSolver.Domain.Repositories.Ticket;
using TicketSolver.Infra.EntityFramework.Persistence.Contexts.Interfaces;

namespace TicketSolver.Infra.EntityFramework.Repositories.Chat;

public class ChatRepository<TTickets>(IEfContext context, ITicketsRepository<TTickets> ticketsRepository) 
    : EFRepositoryBase<TicketChat>(context), IChatRepository
    where TTickets : Tickets
{
    public async Task<IEnumerable<TicketChat>> ExecuteQueryAsync(IQueryable<TicketChat> query, CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TicketChat?> GetChatByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Ticket)
            .FirstOrDefaultAsync(c => c.TicketId == ticketId, cancellationToken);
    }
    



    public async Task<TicketChat> AddMessageToChatAsync(int ticketId, Message message, CancellationToken cancellationToken = default)
    {
        var chat = await GetChatByTicketIdAsync(ticketId, cancellationToken);
        var ticket = await ticketsRepository.GetByIdAsync(ticketId);

        if (chat == null)
        {
            chat = new TicketChat
            {
                Ticket       = ticket,
                TicketId     = ticketId,
                ChatHistory  = "[]",
                TotalMessages= 0,
                CreatedAt    = DateTime.Now,
                UpdatedAt    = DateTime.Now
            };

            await DbSet.AddAsync(chat, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        
            // Recarregar o chat após criação
            chat = await GetChatByTicketIdAsync(ticketId, cancellationToken);
        }

        // Debug: Verificar estado atual
        Console.WriteLine($"=== DEBUG AddMessage ===");
        Console.WriteLine($"Chat ID: {chat?.Id}");
        Console.WriteLine($"ChatHistory antes: {chat?.ChatHistory}");
        Console.WriteLine($"Messages.Count antes: {chat?.Messages?.Count}");

        // Obter mensagens atuais e adicionar a nova
        var currentMessages = chat?.Messages ?? new List<Message>();
        currentMessages.Add(message);

        // Atualizar propriedades
        chat!.Messages = currentMessages; // Isso vai atualizar o ChatHistory via setter
        chat.TotalMessages = currentMessages.Count;
        chat.LastMessageAt = message.Timestamp;
        chat.UpdatedAt = DateTime.Now;

        Console.WriteLine($"ChatHistory depois: {chat.ChatHistory}");
        Console.WriteLine($"Messages.Count depois: {chat.Messages.Count}");
        Console.WriteLine($"Nova mensagem: {message.text}");

        // Salvar no banco
        DbSet.Update(chat);
        await Context.SaveChangesAsync(cancellationToken);

        Console.WriteLine("=== Mensagem salva ===");
    
        return chat;
    }

    public async Task<TicketChat> UpdateChatHistoryAsync(int ticketId, List<Message> messages, CancellationToken cancellationToken = default)
    {
        var chat = await GetChatByTicketIdAsync(ticketId, cancellationToken);
        
        if (chat == null)
        {
            throw new InvalidOperationException($"Chat not found for TicketId: {ticketId}");
        }

        chat.Messages = messages;
        chat.TotalMessages = messages.Count;
        chat.LastMessageAt = messages.LastOrDefault()?.Timestamp;
        chat.UpdatedAt = DateTime.Now;

        DbSet.Update(chat);
        await Context.SaveChangesAsync(cancellationToken);

        return chat;
    }

    public async Task<IEnumerable<TicketChat>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default)
    {
        var chats = await DbSet
            .Include(c => c.Ticket)
            .Where(c => c.ChatHistory != "[]" && c.ChatHistory != null)
            .ToListAsync(cancellationToken);

        // Filtrar chats que têm mensagens não lidas para o usuário específico
        var chatsWithUnread = chats.Where(chat =>
        {
            var messages = chat.Messages;
            return messages.Any(m => !m.IsRead && 
                                   m.SenderId != userId && 
                                   m.SenderType != userType);
        }).ToList();

        return chatsWithUnread;
    }

    public async Task<IEnumerable<TicketChat>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Ticket)
            .Where(c => c.LastMessageAt.HasValue)
            .OrderByDescending(c => c.LastMessageAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkMessagesAsReadAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default)
    {
        var chat = await GetChatByTicketIdAsync(ticketId, cancellationToken);
        
        if (chat == null) return;

        var messages = chat.Messages;
        var hasChanges = false;

        foreach (var message in messages.Where(m => !m.IsRead && 
                                                   m.SenderId != userId && 
                                                   m.SenderType != userType))
        {
            message.IsRead = true;
            hasChanges = true;
        }

        if (hasChanges)
        {
            chat.Messages = messages;
            chat.UpdatedAt = DateTime.Now;
            
            DbSet.Update(chat);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<TicketChat>> GetChatsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Ticket)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ChatExistsForTicketAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(c => c.TicketId == ticketId, cancellationToken);
    }

    public async Task<IEnumerable<TicketChat>> GetChatsBySenderTypeAsync(string senderType, CancellationToken cancellationToken = default)
    {
        var chats = await DbSet
            .Include(c => c.Ticket)
            .Where(c => c.ChatHistory != "[]" && c.ChatHistory != null)
            .ToListAsync(cancellationToken);

        // Filtrar chats que contêm mensagens do tipo de remetente especificado
        var filteredChats = chats.Where(chat =>
        {
            var messages = chat.Messages;
            return messages.Any(m => m.SenderType.Equals(senderType, StringComparison.OrdinalIgnoreCase));
        }).ToList();

        return filteredChats;
    }

    public async Task<ChatStatistics> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var chat = await GetChatByTicketIdAsync(ticketId, cancellationToken);
        
        if (chat == null)
        {
            return new ChatStatistics
            {
                TotalMessages = 0,
                UnreadMessages = 0,
                LastMessageAt = null,
                CreatedAt = DateTime.Now,
                LastSenderType = string.Empty,
                LastSenderName = string.Empty
            };
        }

        var messages = chat.Messages;
        var lastMessage = messages.LastOrDefault();
        var unreadCount = messages.Count(m => !m.IsRead);

        return new ChatStatistics
        {
            TotalMessages = chat.TotalMessages,
            UnreadMessages = unreadCount,
            LastMessageAt = chat.LastMessageAt,
            CreatedAt = chat.CreatedAt,
            LastSenderType = lastMessage?.SenderType ?? string.Empty,
            LastSenderName = lastMessage?.SenderName ?? string.Empty
        };
    }

    public async Task<TicketChat> UpdateAsync(TicketChat chat, CancellationToken cancellationToken = default)
    {
            var existingChat = await GetChatByTicketIdAsync(chat.TicketId, cancellationToken);
            if (existingChat == null)
            {
                throw new InvalidOperationException($"Chat não encontrado no ticket: {chat.TicketId}");
            }
            existingChat.ChatHistory = chat.ChatHistory;
            existingChat.TotalMessages = chat.TotalMessages;
            existingChat.LastMessageAt = chat.LastMessageAt;
            existingChat.IsArchived = chat.IsArchived;
            existingChat.UpdatedAt = DateTime.Now;
            DbSet.Update(existingChat);
            await Context.SaveChangesAsync(cancellationToken);
            return existingChat;
    }

}
