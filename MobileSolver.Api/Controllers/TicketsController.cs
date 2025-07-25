using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileSolver.Api.Models;
using MobileSolver.Domain.Persistence.Entities;
using TicketSolver.Api.Exceptions;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models;
using TicketSolver.Application.Models.User;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace MobileSolver.Api.Controllers;

[ApiController]
public class TicketsController(ITicketsService<MobileTickets> service) : ShellController
{
    [HttpGet]
    [Authorize(Roles = "1")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
        => Ok(await service.GetAllAsync());


    [HttpGet("{id}/")]
    public async Task<ActionResult<Tickets>> GetTicket(int id)
    {
        var t = await service.GetByIdAsync(id);
        return t is not null ? Ok(ApiResponse.Ok(t)) : NotFound(ApiResponse.Fail("Ticket não encontrado!"));
    }


    [HttpPost]
    [Authorize(Roles = "1,3")]
    public async Task<ActionResult<Tickets>> PostTicket(TicketDTO ticket)
    {
        var created = await service.CreateAsync(ticket, AuthenticatedUser.UserId);
        return Ok(ApiResponse.Ok(CreatedAtAction(nameof(GetTicket),
            new { id = created.Id }, created)));
    }


    [HttpPut("{id}/")]
    public async Task<IActionResult> PutTicket(int id, TicketDTO ticket)
    {
        try
        {
            var updateAsync = await service.UpdateAsync(ticket, id);
            return Ok(ApiResponse.Ok(updateAsync));
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "1,2")]
    [HttpDelete("{id}/")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var status = await service.DeleteAsync(id);
        if (status)
            return NoContent();
        return NotFound(ApiResponse.Fail("Ticket não encontrado!"));
    }


    [HttpGet("technician/")]
    public async Task<ActionResult<PaginatedResponse<Tickets>>> GetAllByTech(CancellationToken cancellationToken,
        [FromQuery] PaginatedQuery paginatedQuery, [FromQuery] bool history)
    {
        try
        {
            var tickets =
                await service.GetAllByTechAsync(cancellationToken, AuthenticatedUser.UserId, paginatedQuery, history);
            return Ok(ApiResponse.Ok(tickets));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("technician/performance")]
    public async Task<ActionResult<TechnicianPerformance>> GetTechnicianPerformanceAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var performance = await service.GetTechPerformanceAsync(cancellationToken, AuthenticatedUser.UserId);
            return Ok(ApiResponse.Ok(performance));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("technician/counters")]
    public async Task<ActionResult<TechnicianCounters>> GetTechnicianCountersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var performance = await service.GetTechCountersAsync(cancellationToken, AuthenticatedUser.UserId);
            return Ok(ApiResponse.Ok(performance));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("technician/{techId}/")]
    public async Task<ActionResult<PaginatedResponse<Tickets>>> GetAllByTech(string techId,
        CancellationToken cancellationToken, [FromQuery] PaginatedQuery paginatedQuery, [FromQuery] bool history)
    {
        try
        {
            var tickets = await service.GetAllByTechAsync(cancellationToken, techId, paginatedQuery, history);
            return Ok(ApiResponse.Ok(tickets));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("user/")]
    public async Task<IActionResult> GetAllByUser(CancellationToken cancellationToken,
        [FromQuery] PaginatedQuery paginatedQuery)
    {
        try
        {
            var tickets = await service.GetAllByUserAsync(cancellationToken, AuthenticatedUser.UserId, paginatedQuery);
            return Ok(ApiResponse.Ok(tickets));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("user/counters")]
    public async Task<ActionResult<TechnicianCounters>> GetUserCountersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var performance = await service.GetUserCountersAsync(cancellationToken, AuthenticatedUser.UserId);
            return Ok(ApiResponse.Ok(performance));
        }
        catch (UserNotFoundException)
        {
            throw new NotFoundException("Usuário não encontrado!");
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("user/{id}/")]
    public async Task<IActionResult> GetAllByUser(CancellationToken cancellationToken, string id,
        [FromQuery] PaginatedQuery paginatedQuery)
    {
        try
        {
            var tickets = await service.GetAllByUserAsync(cancellationToken, id, paginatedQuery);
            if (tickets is not null)
                return Ok(ApiResponse.Ok(tickets));
            return NotFound(ApiResponse.Ok(new { }, "Nenhum ticket encontrado"));
        }
        catch (TicketException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}/status/{status}/")]
    public async Task<ActionResult> UpdateTicketStatus(int id, short status)
    {
        var ok = await service.UpdateTicketStatusAsync(id, status);
        if (!ok)
            return BadRequest(new { message = "ID inválido, status fora do intervalo (0–3) ou ticket não existe." });

        return Ok(ApiResponse.Ok("", "Status atualizado com sucesso!"));
    }

    [HttpPut("{ticketId:int}/assign/users")]
    public async Task<IActionResult> AssignTicketToTech([FromRoute] int ticketId, [FromBody] List<string> userIds,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await service.AssignedTechTicketAsync(cancellationToken, ticketId, userIds.ToHashSet());
            if (!ok)
                return BadRequest(ApiResponse.Fail("Ticket ou técnico não encontrado (IDs inválidos)."));
            return Ok(ApiResponse.Ok("", "Ticket atribuído com sucesso!"));
        }
        catch (TicketException e)
        {
            return BadRequest(ApiResponse.Fail(e.Message));
        }
    }
    
    [HttpPatch("{ticketId:int}/unassign/{userId}")]
    public async Task<IActionResult> AssignTicketToTech([FromRoute] int ticketId, [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.UnassignTechAsync(cancellationToken, ticketId, userId);
            return Ok(ApiResponse.Ok("", "Ticket atribuído com sucesso!"));
        }
        catch (TicketException e)
        {
            return BadRequest(ApiResponse.Fail(e.Message));
        }
    }

    [HttpGet("{ticketId:int}/users/")]
    public async Task<IActionResult> GetTicketUsers(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetTicketUsersAsync(cancellationToken, ticketId);
            return Ok(ApiResponse.Ok(result));
        }
        catch (TicketException e)
        {
            return BadRequest(ApiResponse.Fail(e.Message));
        }
    }

    [HttpGet("{ticketId:int}/users/available")]
    public async Task<IActionResult> GetAvailableTicketUsers(int ticketId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetAvailableTicketUsersAsync(cancellationToken, ticketId);
            return Ok(ApiResponse.Ok(result));
        }
        catch (TicketException e)
        {
            return BadRequest(ApiResponse.Fail(e.Message));
        }
    }

    [HttpGet("counts/")]
    public async Task<IActionResult> GetCounts()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if ((id is null) || (role is null))
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        if (AuthenticatedUser.DefUserType == eDefUserTypes.Technician)
        {
            try
            {
                var counts = await service.GetCountsasync(id);
                if (string.IsNullOrEmpty(counts))
                {
                    return NotFound(ApiResponse.Fail("Tecnico {id} não encontrado ou sem tickets. "));
                }

                return Ok(ApiResponse.Ok(counts));
            }
            catch (TicketException ex)
            {
                return NotFound(ApiResponse.Fail(ex.Message));
            }
        }

        if (role is not ("User" or "Admin"))
            return BadRequest(ApiResponse.Fail("Erro ao realizar contagem."));

        try
        {
            var counts = await service.GetCountsasync(id);
            if (string.IsNullOrEmpty(counts))
            {
                return NotFound(new { message = $"Usuário {id} não encontrado ou sem tickets." });
            }

            return Ok(ApiResponse.Ok(counts));
        }
        catch (TicketException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("latest/")]
    public async Task<IActionResult> GetLatest()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);
        if ((id is null) || (role is null))
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }

        if (role == "Technician")
        {
            try
            {
                var tickets = await service.GetLatestTechAsync(id);
                if (tickets is null)
                {
                    return NotFound(ApiResponse.Fail("Tecnico {id} não encontrado ou sem tickets. "));
                }

                return Ok(ApiResponse.Ok(tickets));
            }
            catch (TicketException ex)
            {
                return NotFound(ApiResponse.Fail(ex.Message));
            }
        }

        if (role is not ("User" or "Admin")) return BadRequest(ApiResponse.Fail("Erro ao buscar ultimo ticket."));
        {
            try
            {
                var tickets = await service.GetLatestUserAsync(id) ?? [];
                if (tickets is null)
                {
                    return NotFound(new { message = $"Usuário {id} não encontrado ou sem tickets." });
                }

                return Ok(ApiResponse.Ok(tickets));
            }
            catch (TicketException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}