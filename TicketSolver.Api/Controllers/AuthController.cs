using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Api.Models;
using TicketSolver.Api.Models.Auth;
using TicketSolver.Domain.Exceptions;
using TicketSolver.Domain.Services.Interfaces;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IAuthService authService,
    IConfiguration configuration
) : Controller
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authService.GetRegistrationUser(model.Key, model.Email, cancellationToken);
            var result = await userManager.CreateAsync(user.IdentityUser, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse.Fail(
                        "Erro de autenticação",
                        result.Errors.Select(e => e.Description).ToList()
                    )
                );
            }

            var success = await authService.AttachIdentityUserToUserAsync(
                user.IdentityUser.Id,
                user.UserId,
                cancellationToken
            );

            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse.Fail(
                        "Usuário não encontrado!"
                    )
                );
            }


            return Ok(ApiResponse.Ok(new { }));
        }
        catch (HttpException httpException) // Unauthorized; Forbidden
        {
            return StatusCode((int)httpException.Status, ApiResponse.Fail(httpException.Message));
        }
        catch (Exception e)
        {
            return BadRequest(new ApiResponse(false, e.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByEmailAsync(model.Email);
        if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, model.Password))
            return Unauthorized();

        var userId = await authService.GetUserIdByIdentityUserAsync(identityUser.Id, cancellationToken);
        if (userId <= 0) return Unauthorized();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["JwtKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, identityUser.Email),
                new Claim(ClaimTypes.Name, identityUser.Id),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { token = tokenHandler.WriteToken(token) });
    }
}