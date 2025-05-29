namespace TicketSolver.Application.Models.Chat;

public class ChatHistoryResponseDto
{
    public int TicketId { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = [];
    public int TotalMessages { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
    public DateTime? LastMessageAt { get; set; }
}


