namespace TicketSolver.Application.Models.Chat;

public class ChatMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SenderId { get; set; } = string.Empty;
    public string SenderType { get; set; } = "User"; // User, Technician, AI
    public string SenderName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, File, Image
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}