using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Application.Services.Ticket.Interfaces;
using TicketSolver.Domain.Enums;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using System.Security.Claims;
using TicketSolver.Application.Exceptions.Ticket;
using TicketSolver.Application.Models;

namespace TicketSolver.Api.Controllers;

[ApiController]
public class TicketsController(ITicketsService service) : ShellController
{   
    
    [HttpGet]
    [Authorize(Roles = "1")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
        => Ok(await service.GetAllAsync());

    
    [HttpGet("{id}")]
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
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
        var created = await service.CreateAsync(ticket,userId);
        return Ok(ApiResponse.Ok(CreatedAtAction(nameof(GetTicket),
            new { id = created.Id }, created)));
    }


    [HttpPut("{id}")]
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
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
        => await service.DeleteAsync(id)
            ? NoContent()
            : NotFound();

    [HttpGet("tech/{id}/tickets")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetAllByTech(string id)
    {
        var tickets = await service.GetAllByTechAsync(id);
        if (tickets is null)
            return NotFound();
        return Ok(tickets);
    }



    
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetAllByUser(string id)
    {
        var list = await service.GetAllByUserAsync(id);
        
        if (list == null)
            return NotFound();
        
        return Ok(
            ApiResponse.Ok(list)
            );
    }
    
    [HttpPut("{id}/status/{status}")]
    public async Task<ActionResult> UpdateTicketStatus(int id, short status)
    {
        var ok = await service.UpdateTicketStatusAsync(id, status);
        if (!ok)
            
            return BadRequest(new { message = "ID inválido, status fora do intervalo (0–3) ou ticket não existe." });
        
        return NoContent();
    }
    
     [HttpPut("{id}/assign/{techId}")]
        public async Task<IActionResult> AssignTicketToTech(int id, string techId)
        {
            var ok = await service.AssignedTechTicketAsync(id, techId);
            if (!ok)
                
                return BadRequest(new { 
                    message = "Ticket ou técnico não encontrado (IDs inválidos)." 
                });
            
            return NoContent();
        }
        
        [HttpGet("{id}/counts")]
        public async Task<IActionResult> GetCounts(string id)
        {
            
            var counts = await service.GetCountsasync(id);


            if (string.IsNullOrEmpty(counts))
            {
                return NotFound(new { message = $"Usuário {id} não encontrado ou sem tickets." });
            }

            return Ok(
                ApiResponse.Ok(counts)
            );
        }
        
        [HttpGet("{id}/latest")]
        public async Task<IActionResult> GetLatest(string id)
        {
            var tickets = await service.GetLatestUserAsync(id);
            
            if (tickets == null)
                tickets = Enumerable.Empty<Tickets>();
            
            return Ok(ApiResponse.Ok(tickets));
        }
        
        [HttpGet("tech/{id}/latest")]
        public async Task<IActionResult> GetLatestByTech(string id)
        {
            var tickets = await service.GetLatestTechAsync(id);
            
            if (tickets == null)
                tickets = Enumerable.Empty<Tickets>();

            return Ok(ApiResponse.Ok(tickets));
        }
}