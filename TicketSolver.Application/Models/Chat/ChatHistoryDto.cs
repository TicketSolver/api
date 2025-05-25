namespace TicketSolver.Application.Models.Chat;

public class ChatHistoryDto
{
    public int TicketId { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string TicketStatus { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsTechnician { get; set; }
}