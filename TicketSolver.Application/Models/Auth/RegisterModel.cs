namespace TicketSolver.Application.Models.Auth;

public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public Guid Key { get; set; }
}