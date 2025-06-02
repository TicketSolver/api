namespace TicketSolver.Application.Models.Chat;

public class ChatStatisticsDto
{
    public int TicketId { get; set; }
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string LastSenderType { get; set; } = string.Empty;
    public string LastSenderName { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public Dictionary<string, int> MessagesByType { get; set; } = [];
    public Dictionary<string, int> MessagesBySender { get; set; } = [];
}