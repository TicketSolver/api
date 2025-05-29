namespace TicketSolver.Domain.Models.Ticket;

public class TicketShort
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public short Status { get; set; }
    public short Priority { get; set; }
    public short Category { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public UserShort? AssignedTo { get; set; }
    public UserShort CreatedBy { get; set; }
}