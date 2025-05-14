using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class Attachments : EntityDates
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public int TicketId { get; set; }
    [ForeignKey("TicketId")] public Tickets Ticket { get; set; }
}