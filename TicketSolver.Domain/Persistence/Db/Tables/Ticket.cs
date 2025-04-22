namespace TicketSolver.Domain.Persistence.Db.Tables;

public class Tickets
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public short Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedById { get; set; }
    public int? AssignedToId { get; set; }
}
