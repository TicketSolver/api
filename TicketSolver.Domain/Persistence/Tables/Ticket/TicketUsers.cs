using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class TicketUsers : EntityDates
{
    public int Id { get; set; }
    public DateTime AddedAt { get; set; }

    public string UserId { get; set; }
    [ForeignKey("UserId")] public Users User { get; set; }

    public int TicketId { get; set; }
    [ForeignKey("TicketId")] public Tickets Ticket { get; set; }

    public short DefTicketUserRoleId { get; set; }
    [ForeignKey("DefTicketUserRoleId")] public DefTicketUserRoles DefTicketUserRole { get; set; }
}