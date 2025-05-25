namespace TicketSolver.Application.Models.Chat;


public class ChatParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public DateTime? LastSeenAt { get; set; }
    public int MessageCount { get; set; }
}