using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Db.Tables;

public class Tickets
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedById { get; set; }
    public int? AssignedToId { get; set; }
}
