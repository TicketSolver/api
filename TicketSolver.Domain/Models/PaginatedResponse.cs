namespace TicketSolver.Domain.Models;

public class PaginatedResponse<T>
{

    public int Count { get; set; }
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PaginatedResponse()
    {
        Items = [];
        Count = 0;
        Page = 1;
        PageSize = 1;
    }
    
    public PaginatedResponse(List<T> items, int page, int pageSize, int count)
    {
        Count = count;
        Items = items;
        Page = page;
        PageSize = pageSize;
    }
    
}