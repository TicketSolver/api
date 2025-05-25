namespace TicketSolver.Application.Models.Chat;

public class ChatMessageResponseDto
{
    public string Id { get; set; } = string.Empty;
    public int TicketId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime Timestamp { get; set; }
}