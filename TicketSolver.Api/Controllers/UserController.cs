using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Exceptions;
using TicketSolver.Api.Models;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Services.User.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Api.Controllers;

public class UsersController(IUsersService usersService) : ShellController
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Users>>> GetUsers(CancellationToken cancellationToken,
        PaginatedQuery query)
    {
        var users = await usersService.ListUsersAsync(query, cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Users>> GetUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await usersService.GetUserByIdAsync(id, cancellationToken);
            return Ok(
                ApiResponse.Ok(user)
            );
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Users>> PostUser(Users user, CancellationToken cancellationToken)
    {
        try
        {
            await usersService.CreateUserAsync(user, cancellationToken);
            return Ok(
                ApiResponse.Ok(user)
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUser(string id, Users body, CancellationToken cancellationToken)
    {
        var user = await usersService.UpdateUserAsync(id, body, AuthenticatedUser, cancellationToken);
        
        return Ok(ApiResponse.Ok(user));
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (AuthenticatedUser.UserId != id && AuthenticatedUser.DefUserType != eDefUserTypes.Admin)
            throw new ForbiddenException("Você não possui permissão para editar este usuário!");
        
        // usersService.DeleteUserAsync
        
        return Ok(ApiResponse.Ok(new {}));
    }
}