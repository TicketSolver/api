namespace TicketSolver.Application.Models.Chat;

public class ChatSearchRequestDto
{
    public int? TicketId { get; set; }
    public string? SearchText { get; set; }
    public string? SenderType { get; set; }
    public string? SenderId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MessageType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}