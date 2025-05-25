using Microsoft.EntityFrameworkCore;
using TicketSolver.Application.Models.Chat;
using TicketSolver.Domain.Persistence.Tables.Chat;
using TicketSolver.Domain.Repositories;
using TicketSolver.Domain.Repositories.Chat;
using TicketSolver.Infra.EntityFramework.Persistence;
using ChatMessageDto = TicketSolver.Domain.Persistence.Tables.Chat.ChatMessageDto;

namespace TicketSolver.Infra.EntityFramework.Repositories.Chat;

public class ChatRepository(EfContext context) : IRepositoryBase<IChatRepository>
{
        private readonly EfContext _context;
        
        public async Task<TicketChat?> GetChatByTicketIdAsync(int ticketId)
        {
            return await _context.Chats
                .AsNoTracking()
                .FirstOrDefaultAsync(tc => tc.TicketId == ticketId);
        }

        public async Task<TicketChat> CreateChatAsync(int ticketId)
        {
            var chat = new TicketChat
            {
                TicketId = ticketId,
                ChatHistory = "[]",
                TotalMessages = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            
            return chat;
        }

        public async Task<TicketChat> AddMessageToChatAsync(int ticketId, ChatMessageDto message)
        {
            // ✅ Buscar ou criar chat
            var chat = await _context.Chats
                .FirstOrDefaultAsync(tc => tc.TicketId == ticketId);

            if (chat == null)
            {
                chat = await CreateChatAsync(ticketId);
            }

            // ✅ Adicionar mensagem à lista
            var messages = chat.Messages;
            messages.Add(message);
            chat.Messages = messages;

            // ✅ Atualizar contadores
            chat.TotalMessages = messages.Count;
            chat.LastMessageAt = message.Timestamp;
            chat.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            return chat;
        }

        public async Task<TicketChat> MarkMessagesAsReadAsync(int ticketId, int currentUserId)
        {
            var chat = await _context.Chats
                .FirstOrDefaultAsync(tc => tc.TicketId == ticketId);

            if (chat != null)
            {
                // ✅ Marcar mensagens como lidas
                var messages = chat.Messages;
                foreach (var msg in messages.Where(m => m.SenderId != currentUserId && !m.IsRead))
                {
                    msg.IsRead = true;
                }
                
                chat.Messages = messages;
                chat.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return chat ?? throw new ArgumentException("Chat não encontrado");
        }

        public async Task<int> GetUnreadCountAsync(int ticketId, int currentUserId)
        {
            var chat = await GetChatByTicketIdAsync(ticketId);
            
            if (chat == null) return 0;

            return chat.Messages
                .Count(m => m.SenderId != currentUserId && !m.IsRead);
        }
        
        public async Task<TicketPermissionDto> GetTicketPermissionAsync(int ticketId, string currentUserId)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .Select(t => new { t.Id, t.CreatedById, t.AssignedToId, t.Status })
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return new TicketPermissionDto
                {
                    Exists = false,
                    HasPermission = false,
                    TicketStatus = null
                };
            }

            // ✅ Usuario é dono do ticket OU é o técnico atribuído
            var hasPermission = ticket.CreatedById == currentUserId || 
                               ticket.AssignedToId == currentUserId;

            return new TicketPermissionDto
            {
                Exists = true,
                HasPermission = hasPermission,
                TicketStatus = ticket.Status.ToString(),
                IsOwner = ticket.CreatedById == currentUserId,
                IsTechnician = ticket.AssignedToId == currentUserId
            };
        }
        
        public async Task<string> GetUserNameAsync(string userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.FullName, u.Email })
                .FirstOrDefaultAsync();

            if (user == null)
                return $"Usuário #{userId}";

            var fullName = $"{user.FullName}".Trim();
            return (string.IsNullOrEmpty(fullName) ? user.Email : fullName) ?? string.Empty;
        }
        
        public async Task UpdateTicketLastActivityAsync(int ticketId)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket != null)
            {
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<IChatRepository> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IChatRepository>> ExecuteQueryAsync(IQueryable<IChatRepository> query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
}
