namespace TicketSolver.Api.Models.Auth;

public class AuthenticatedUserModel
{
    public string Email { get; set; }
    public string IdentityUserId { get; set; }
    public int UserId { get; set; }
}