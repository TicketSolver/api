namespace TicketSolver.Application.Models.Chat;

public class ChatResponseDto
{
    public int TicketId { get; set; }
    public bool IsNewChat { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalMessages { get; set; }
}