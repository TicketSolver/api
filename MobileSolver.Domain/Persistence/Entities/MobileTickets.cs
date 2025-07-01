using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace MobileSolver.Domain.Persistence.Entities;

[Table("Tickets")]
public class MobileTickets : Tickets
{
    public string? DeviceModel { get; set; }
    public string? Version { get; set; }
    public string? Application { get; set; }
}