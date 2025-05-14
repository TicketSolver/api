namespace TicketSolver.Domain.Models;

public class PaginatedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int Skip => (Page - 1) * PageSize;
}