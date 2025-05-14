using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Defs;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShellController : Controller
{
    protected AuthenticatedUser AuthenticatedUser
    {
        get
        {
            try
            {
                return new()
                {
                    Email = User.FindFirst(ClaimTypes.Email)!.Value,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                    DefUserType = (eDefUserTypes)short.Parse(User.FindFirst(ClaimTypes.Role)!.Value),
                };
            }
            catch
            {
                return new()
                {
                    IsAuthenticated = false,
                };
            }
        }
    }
}