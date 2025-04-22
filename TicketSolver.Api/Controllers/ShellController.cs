using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models.Auth;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShellController : Controller
{
    protected AuthenticatedUserModel AuthenticatedUser
    {
        get
        {
            try
            {
                return new()
                {
                    Email = User.FindFirst(ClaimTypes.Email)!.Value,
                    IdentityUserId = User.FindFirst(ClaimTypes.Name)!.Value,
                    UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                };
            }
            catch
            {
                return new();
            }
        }
    }
}