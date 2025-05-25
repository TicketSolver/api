namespace TicketSolver.Application.Models.Chat;

public class TicketPermissionDto
{
    public bool Exists { get; set; }
    public bool HasPermission { get; set; }
    public string? TicketStatus { get; set; }
    public bool IsOwner { get; set; }
    public bool IsTechnician { get; set; }
}