namespace TicketSolver.Application.Models.Chat;

public class MarkAsReadRequestDto
{
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
}