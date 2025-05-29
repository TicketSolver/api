namespace TicketSolver.Application.Models.Chat;

public class ChatSummaryDto
{
    public int TicketId { get; set; }
    public string TicketTitle { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string LastMessageText { get; set; } = string.Empty;
    public string LastSenderName { get; set; } = string.Empty;
    public string LastSenderType { get; set; } = string.Empty;
    
}