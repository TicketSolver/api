using TicketSolver.Application.Models.Chat;

namespace TicketSolver.Application.Services.Chat.Interfaces;

public interface IChatService
{
    Task<ChatMessageResponseDto> SendMessageAsync(SendMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<ChatHistoryResponseDto> GetChatHistoryAsync(int ticketId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task MarkMessagesAsReadAsync(MarkAsReadRequestDto request, CancellationToken cancellationToken = default);

    Task<IEnumerable<ChatSummaryDto>> GetChatsWithUnreadMessagesAsync(string userId, string userType, CancellationToken cancellationToken = default);

    Task<IEnumerable<ChatSummaryDto>> GetRecentChatsAsync(int limit = 10, CancellationToken cancellationToken = default);

    Task<ChatStatisticsDto> GetChatStatisticsAsync(int ticketId, CancellationToken cancellationToken = default);

    Task<ChatResponseDto> StartChatAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessChatAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessageDto>> SearchMessagesAsync(ChatSearchRequestDto request, CancellationToken cancellationToken = default);
    Task<int> GetUnreadMessageCountAsync(int ticketId, string userId, string userType, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatParticipantDto>> GetChatParticipantsAsync(int ticketId, CancellationToken cancellationToken = default);
    Task<ChatMessageResponseDto> SendSystemMessageAsync(SendSystemMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<ChatActivitySummaryDto> GetChatActivitySummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task ArchiveChatAsync(int ticketId, bool isArchived, CancellationToken cancellationToken = default);
    Task<ChatInfoDto> GetChatInfoAsync(int ticketId, CancellationToken cancellationToken = default);
}
