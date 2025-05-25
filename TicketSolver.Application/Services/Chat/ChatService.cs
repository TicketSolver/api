using TicketSolver.Application.Models.Chat;
using TicketSolver.Application.Services.Chat.Interfaces;
using TicketSolver.Domain.Repositories.Chat;

namespace TicketSolver.Application.Services.Chat;

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<ChatHistoryDto> GetChatHistoryAsync(int ticketId, string currentUserId, string userRole)
        {
            
            var permission = await chatRepository.GetTicketPermissionAsync(ticketId, currentUserId);

            if (!permission.Exists)
                throw new ArgumentException("Ticket não encontrado");

            // ✅ Para técnicos, sempre permitir (podem ver qualquer ticket)
            // ✅ Para usuários, só o próprio ticket
            if (userRole == "User" && !permission.IsOwner)
                throw new UnauthorizedAccessException("Acesso negado");

            // ✅ Buscar dados do chat
            var chat = await chatRepository.GetChatByTicketIdAsync(ticketId);
            var unreadCount = await chatRepository.GetUnreadCountAsync(ticketId, currentUserId);

            return new ChatHistoryDto
            {
                TicketId = ticketId,
                Messages = chat?.Messages ?? new List<ChatMessageDto>(),
                TotalMessages = chat?.TotalMessages ?? 0,
                UnreadCount = unreadCount,
                LastMessageAt = chat?.LastMessageAt,
                TicketStatus = permission.TicketStatus ?? "Unknown",
                IsOwner = permission.IsOwner,
                IsTechnician = permission.IsTechnician
            };
        }

    public Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, string currentUserId, string userRole)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkMessagesAsReadAsync(int ticketId, string currentUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, int currentUserId, string userRole)
        {
            // ✅ Verificar permissões
            var permission = await chatRepository.GetTicketPermissionAsync(dto.TicketId, currentUserId);

            if (!permission.Exists)
                throw new ArgumentException("Ticket não encontrado");

            if (userRole == "User" && !permission.IsOwner)
                throw new UnauthorizedAccessException("Acesso negado");

            // ✅ Buscar nome do usuário
            var senderName = await chatRepository.GetUserNameAsync(currentUserId);

            // ✅ Criar mensagem
            var message = new ChatMessageDto
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = currentUserId,
                SenderType = DetermineSenderType(userRole, permission),
                SenderName = senderName,
                Message = dto.Message.Trim(),
                MessageType = dto.MessageType ?? "Text",
                AttachmentUrl = dto.AttachmentUrl,
                IsRead = false,
                Timestamp = DateTime.UtcNow
            };

            // ✅ Adicionar ao chat via repository
            await chatRepository.AddMessageToChatAsync(dto.TicketId, message);

            // ✅ Atualizar última atividade do ticket
            await chatRepository.UpdateTicketLastActivityAsync(dto.TicketId);

            return message;
        }

        public async Task<bool> MarkMessagesAsReadAsync(int ticketId, int currentUserId)
        {
            // ✅ Verificar se o ticket existe e usuário tem permissão
            var permission = await chatRepository.GetTicketPermissionAsync(ticketId, currentUserId);

            if (!permission.Exists)
                throw new ArgumentException("Ticket não encontrado");

            if (!permission.HasPermission)
                throw new UnauthorizedAccessException("Acesso negado");

            // ✅ Marcar como lidas
            await chatRepository.MarkMessagesAsReadAsync(ticketId, currentUserId);
            
            return true;
        }

        // ✅ Método privado para determinar tipo do remetente
        private string DetermineSenderType(string userRole, TicketPermissionDto permission)
        {
            if (userRole == "Admin" || userRole == "Technician")
                return "Technician";
            
            if (permission.IsTechnician)
                return "Technician";
                
            return "User";
        }
    }
}