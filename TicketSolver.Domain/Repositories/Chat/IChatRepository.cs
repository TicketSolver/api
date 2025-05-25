using TicketSolver.Domain.Models.Chat;
using TicketSolver.Domain.Persistence.Tables.Chat;

namespace TicketSolver.Domain.Repositories.Chat;

public interface IChatRepository : IRepositoryBase<TicketChat>
{
    Task<TicketChat?> GetChatByTicketIdAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<TicketChat> AddMessageToChatAsync(int ticketId, Message message, CancellationToken cancellationToken = default);
    Task<TicketChat> UpdateChatHistoryAsync(int ticketId, List<Message> messages, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketChat>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketChat>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task MarkMessagesAsReadAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketChat>> GetChatsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> ChatExistsForTicketAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketChat>> GetChatsBySenderTypeAsync(string senderType, CancellationToken cancellationToken = default);
    Task<ChatStatistics> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<TicketChat> UpdateAsync(TicketChat chat, CancellationToken cancellationToken = default);

}


