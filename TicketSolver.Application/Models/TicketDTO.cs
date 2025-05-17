using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Application.Models;

public class TicketDTO
{

    public string Title { get; set; }
    public string? Description { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public short Category { get; set; }

}