
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSolver.Domain.Persistence;
using TicketSolver.Domain.Persistence.Db.Tables;

namespace TicketSolver.Api.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController(EFContext context) : ControllerBase
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
        public async Task<ActionResult<Tickets>> PostTicket(Tickets ticket)
        {
            ticket.CreatedAt = DateTime.UtcNow;
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Tickets ticket)
        {
            if (id != ticket.Id)
                return BadRequest();

            context.Entry(ticket).State = EntityState.Modified;
            ticket.UpdatedAt = DateTime.UtcNow;

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