namespace TicketSolver.Domain.Persistence.Tables;

public class EntityDates
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}