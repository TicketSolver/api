using TicketSolver.Domain.Persistence.Tables.Chat;

namespace TicketSolver.Domain.Repositories.Chat;

public interface IChatRepository
{
    Task<TicketChat?> GetChatByTicketIdAsync(int ticketId);
    Task<TicketChat> CreateChatAsync(int ticketId);
    Task<TicketChat> AddMessageToChatAsync(int ticketId, ChatMessageDto message);
    Task<TicketChat> MarkMessagesAsReadAsync(int ticketId, string currentUserId);
    Task<int> GetUnreadCountAsync(int ticketId, string currentUserId);
    Task<Object> GetTicketPermissionAsync(int ticketId, string currentUserId);
    Task<TicketChat> MarkMessagesAsReadAsync(int ticketId, int currentUserId);
    Task<int> GetUnreadCountAsync(int ticketId, int currentUserId);
    Task<string> GetUserNameAsync(string userId);
    Task UpdateTicketLastActivityAsync(int ticketId);
}