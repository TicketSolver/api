using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPatch("{id}/status/{status}")]
    public async Task<IActionResult> UpdateTicketStatus(int id, short status)
        => await _service.UpdateTicketStatusAsync(id, status)
            ? NoContent()
            : NotFound();
    
    [HttpPatch("{id}/assigned/{techId}")]
    public async Task<IActionResult> AssignedTechTicket(int id, int techId)
        => await _service.AssignedTechTicketAsync(id, techId)
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
    
    
    
}