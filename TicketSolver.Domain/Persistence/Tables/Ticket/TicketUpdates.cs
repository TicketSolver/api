using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class TicketUpdates : EntityDates
{
    public int Id { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public int TicketId { get; set; }
    [ForeignKey("TicketId")] public Tickets Ticket { get; set; }

    public int UpdatedById { get; set; }
}