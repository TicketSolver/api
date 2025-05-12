using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Ticket;

public class Tickets : EntityDates
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public short Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedById { get; set; }
    public string? AssignedToId { get; set; }

    [ForeignKey("CreatedById")] public Users CreatedBy { get; set; }
    [ForeignKey("AssignedToId")] public Users AssignedTo { get; set; }

    public ICollection<TicketUpdates> TicketUpdates { get; set; } = [];
    public ICollection<TicketUsers> TicketUsers { get; set; } = [];
    public ICollection<Attachments> Attachments { get; set; } = [];
}