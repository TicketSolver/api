using TicketSolver.Domain.Enums;

namespace TicketSolver.Application.Models.Auth;

public class AuthenticatedUser
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public eDefUserTypes DefUserType { get; set; }
    public bool IsAuthenticated { get; set; } = true;
}