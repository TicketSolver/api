using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TicketSolver.Api.Exceptions;
using TicketSolver.Api.Models;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.Auth;
using TicketSolver.Application.Services.Interfaces;
using TicketSolver.Application.Services.User.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IConfiguration configuration
) : Controller
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authService.RegisterUserAsync(model, cancellationToken);
            return Ok(ApiResponse.Ok(new { userId = user.Id }));
        }
        catch (UserRegistrationException e)
        {
            return BadRequest(
                ApiResponse.Fail("Não foi possível registrar o usuário", e.Errors.ToList())
            );
        }
        catch (UserAlreadyRegisteredException)
        {
            throw new BadRequestException("Usuário já registrado!");
        }
        catch (InvalidPublicKeyException)
        {
            throw new UnauthorizedException("Chave pública inválida!");
        }
        catch (UserNotFoundException)
        {
            throw new ForbiddenException("Chave pública inválida!");
        }
    }

    [HttpPost("preregister")]
    public async Task<IActionResult> Register([FromBody] PreRegisterModel model, CancellationToken cancellationToken)
    {
        var result = await authService.PreRegisterUserAsync(model, cancellationToken);
        if (result.Succeeded)
            return Ok(ApiResponse.Ok(new { result }));

        return BadRequest(ApiResponse.Fail(
                "Erro de autenticação",
                result.Errors.Select(e => e.Description).ToList()
            )
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        try
        {
            var token = await authService.LoginUserAsync(model, cancellationToken);
            return Ok(ApiResponse.Ok( token ));
        }
        catch (AuthenticationFailedException e)
        {
            return Unauthorized( 
                ApiResponse.Fail("Login ou senha inválidos!"));
        }
        catch (Exception e)
        {
            return BadRequest(
                ApiResponse.Fail("Erro de autenticação")
            );
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyKey([FromBody] KeyModel key, CancellationToken cancellationToken)
    {
        try
        {
            var tenantkey = await authService.VerifyKeyAsync(key, cancellationToken);
            if (tenantkey == null)
                return NotFound(ApiResponse.Fail("Chave inválida!"));

            return Ok(ApiResponse.Ok(tenantkey));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail("Chave inválida!"));
        }
    }
}