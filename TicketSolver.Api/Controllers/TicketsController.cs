using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Api.Controllers;

public class TicketsController(EFContext context) : ShellController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tickets>>> GetTickets()
    {
        return await context.Tickets.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tickets>> GetTicket(int id)
    {
        var ticket = await context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        return ticket;
    }

    [HttpPost]
    public async Task<ActionResult<Tickets>> PostTicket(Tickets tickets)
    {
        tickets.CreatedAt = DateTime.UtcNow;
        context.Tickets.Add(tickets);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTicket), new { id = tickets.Id }, tickets);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTicket(int id, Tickets tickets)
    {
        if (id != tickets.Id)
            return BadRequest();

        context.Entry(tickets).State = EntityState.Modified;
        tickets.UpdatedAt = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Tickets.Any(e => e.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await context.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound();

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync();

        return NoContent();
    }
}