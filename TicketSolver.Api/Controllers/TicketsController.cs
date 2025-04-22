using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TicketSolver.Api.Models;
using TicketSolver.Domain.Persistence.Tables.Ticket;
using TicketSolver.Domain.Services.Ticket.Interfaces;

namespace TicketSolver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketsService _service;
    public TicketsController(ITicketsService service)
        => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Tickets>> GetTicket(int id)
    {
        var t = await _service.GetByIdAsync(id);
        return t is not null ? Ok(t) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<Tickets>> PostTicket(Tickets ticket)
    {
        var created = await _service.CreateAsync(ticket);
        return CreatedAtAction(nameof(GetTicket),
            new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTicket(int id, Tickets ticket)
        => await _service.UpdateAsync(id, ticket)
            ? NoContent()
            : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
        => await _service.DeleteAsync(id)
            ? NoContent()
            : NotFound();

    [HttpGet("tech/{id}/tickets")]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetAllByTech(int id)
    {
        var tickets = await _service.GetAllByTechAsync(id);
        if (tickets is null)
            return NotFound();
        return Ok(tickets);
    }
    
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetAllByUser(int id)
    {
        var list = await _service.GetAllByUserAsync(id);
        
        if (list == null)
            return NotFound();
        
        return Ok(
            ApiResponse.Ok(list)
            );
    }
    
    [HttpPut("{id}/status/{status}")]
    public async Task<ActionResult> UpdateTicketStatus(int id, short status)
    {
        var ok = await _service.UpdateTicketStatusAsync(id, status);
        if (!ok)
            
            return BadRequest(new { message = "ID inválido, status fora do intervalo (0–3) ou ticket não existe." });
        
        return NoContent();
    }
    
     [HttpPut("{id}/assign/{techId}")]
        public async Task<IActionResult> AssignTicketToTech(int id, int techId)
        {
            var ok = await _service.AssignedTechTicketAsync(id, techId);
            if (!ok)
                
                return BadRequest(new { 
                    message = "Ticket ou técnico não encontrado (IDs inválidos)." 
                });
            
            return NoContent();
        }
        
        [HttpGet("{id}/counts")]
        public async Task<IActionResult> GetCounts(int id)
        {
            
            var counts = await _service.GetCountsasync(id);


            if (string.IsNullOrEmpty(counts))
            {
                return NotFound(new { message = $"Usuário {id} não encontrado ou sem tickets." });
            }

            return Ok(
                ApiResponse.Ok(counts)
            );
        }
        
        [HttpGet("{id}/latest")]
        public async Task<IActionResult> GetLatest(int id)
        {
            var tickets = await _service.GetLatestUserAsync(id);
            
            if (tickets == null)
                tickets = Enumerable.Empty<Tickets>();
            
            return Ok(ApiResponse.Ok(tickets));
        }
        
        [HttpGet("tech/{id}/latest")]
        public async Task<IActionResult> GetLatestByTech(int id)
        {
            var tickets = await _service.GetLatestTechAsync(id);
            
            if (tickets == null)
                tickets = Enumerable.Empty<Tickets>();

            return Ok(ApiResponse.Ok(tickets));
        }
}