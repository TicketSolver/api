using TicketSolver.Application.Models.Chat;

namespace TicketSolver.Application.Services.Chat.Interfaces;

public interface IChatService
{
    Task<ChatHistoryDto> GetChatHistoryAsync(int ticketId, string currentUserId, string userRole);
    Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, string currentUserId, string userRole);
    Task<bool> MarkMessagesAsReadAsync(int ticketId, string currentUserId);
}