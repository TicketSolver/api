using Microsoft.AspNetCore.Identity;

namespace TicketSolver.Domain.Models.Auth;

public class RegisterUserDto
{
    public IdentityUser IdentityUser { get; set; }
    public int UserId { get; set; }
}