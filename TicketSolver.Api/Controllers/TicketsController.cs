using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using System.Security.Claims;
using TicketSolver.Api.Exceptions;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Exceptions.Users;
using TicketSolver.Application.Models;
using TicketSolver.Domain.Models;

namespace TicketSolver.Api.Controllers;

[ApiController]
public class TicketsController(ITicketsService service) : ShellController
{   
    
    [HttpGet]
    [Authorize(Roles = "1")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
        => Ok(await service.GetAllAsync());

    
    [HttpGet("{id}/")]
    public async Task<ActionResult<Tickets>> GetTicket(int id)
    {
        var t = await service.GetByIdAsync(id);
        return t is not null ? 
            Ok(ApiResponse.Ok(t)) : 
            NotFound(ApiResponse.Fail("Ticket não encontrado!"));
    }


    [HttpPost]
    [Authorize(Roles = "1,3")]
    public async Task<ActionResult<Tickets>> PostTicket(TicketDTO ticket)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
        }
        var created = await service.CreateAsync(ticket,userId);
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
    public async Task<ActionResult<IEnumerable<Tickets>>> GetAllByTech(CancellationToken cancellationToken, [FromQuery] PaginatedQuery paginatedQuery)
    {
        try
        {
            var tickets = await service.GetAllByTechAsync(cancellationToken, AuthenticatedUser.UserId, paginatedQuery);
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

    [HttpGet("technician/{userId}/performance")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTechnicianPerformanceAsync(CancellationToken cancellationToken, string userId, [FromQuery] PaginatedQuery paginatedQuery)
    {
        try
        {
            var tickets = await service.GetTechPerformanceAsync(cancellationToken, AuthenticatedUser.UserId);
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
    
    [HttpGet("technician/{techId}/")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetAllByTech(string techId, CancellationToken cancellationToken, [FromQuery] PaginatedQuery paginatedQuery)
    {
        try
        {
            var tickets = await service.GetAllByTechAsync(cancellationToken, techId, paginatedQuery);
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
    public async Task<IActionResult> GetAllByUser(CancellationToken cancellationToken, PaginatedQuery paginatedQuery){
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
    
    [HttpGet("user/{id}/")]
    public async Task<IActionResult> GetAllByUser(CancellationToken cancellationToken, string id, PaginatedQuery paginatedQuery){
        try
        {
            var tickets = await service.GetAllByUserAsync(cancellationToken, id, paginatedQuery);
            if (tickets is not null)
                return Ok(ApiResponse.Ok(tickets));
            return NotFound(ApiResponse.Ok(new {},"Nenhum ticket encontrado"));
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
        
        return Ok(ApiResponse.Ok("","Status atualizado com sucesso!"));
    }
    
     [HttpPut("{id:int}/assign/")]
        public async Task<IActionResult> AssignTicketToTech(int id, CancellationToken cancellationToken)
        {
            var  techId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (techId is null)
            {
                return BadRequest(ApiResponse.Fail("Usuário não autenticado."));
            }

            try
            {
                var ok = await service.AssignedTechTicketAsync(cancellationToken, id, techId);
                if (!ok)
                    return BadRequest(ApiResponse.Fail("Ticket ou técnico não encontrado (IDs inválidos)."));
                return Ok(ApiResponse.Ok("","Ticket atribuído com sucesso!"));
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

            if (role == "Technician")
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

            if (role is not ("User" or "Admin")) return BadRequest(ApiResponse.Fail("Erro ao realizar contagem."));
            {
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