using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Exceptions;
using TicketSolver.Api.Models;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models.User;
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
    public async Task<IActionResult> PatchUser(string id, UserPatchDto body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await usersService.UpdateUserAsync(id, body, AuthenticatedUser, cancellationToken);

            return Ok(ApiResponse.Ok(user));
        }
        catch (Exception e)
        {
            return BadRequest(
                ApiResponse.Fail("Erro ao atualizar usuário: " + e.Message));
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        if (AuthenticatedUser.UserId != id && AuthenticatedUser.DefUserType != eDefUserTypes.Admin)
            throw new ForbiddenException("Você não possui permissão para editar este usuário!");
        try
        {
            var userExists = await usersService.GetUserByIdAsync(id, cancellationToken);
            if (userExists is null)
                return NotFound(ApiResponse.Fail("Usuário não encontrado."));
            var user = await usersService.DeleteUserAsync(id, AuthenticatedUser, cancellationToken);
            if (!user)
                return BadRequest(ApiResponse.Fail("Erro ao deletar usuário."));
        }
        catch (UserNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Usuário não encontrado."));
        }
        
        return Ok(ApiResponse.Ok(new {}));
    }

    [HttpGet("tenant/{tenantId:int}/")]
    public async Task<ActionResult<PaginatedResponse<Users>>> GetUsersByTenant(
        [FromRoute] int tenantId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        if (pageSize <= 0 || page <= 0)
        {
            pageSize = 10;
            page = 1;
        }
        var users = await usersService.GetUsersTenantAsync(tenantId, page, pageSize, cancellationToken);
        if (users is null || users.Items.Count == 0)
            return NotFound(ApiResponse.Fail("Nenhum usuário encontrado para o Tenant especificado."));
        return Ok(ApiResponse.Ok(users));
    }
        
}