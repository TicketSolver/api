namespace TicketSolver.Domain.Models;

public class PaginatedResponse<T>
{
    public int Count { get; set; }
    public List<T> Items { get; set; }
}