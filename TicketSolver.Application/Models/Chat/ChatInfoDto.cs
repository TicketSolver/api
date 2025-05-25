namespace TicketSolver.Application.Models.Chat;

public class ChatInfoDto
{
    public int TicketId { get; set; }
    public int TotalMessages { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasUnreadMessages { get; set; }
    public List<ChatParticipantDto> Participants { get; set; } = [];
}