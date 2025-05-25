namespace TicketSolver.Application.Models.Chat;

public class SendMessageRequestDto
{
    public int TicketId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderType { get; set; } = "User"; // User, Technician, AI
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text"; // Text, File, Image
    public string? AttachmentUrl { get; set; }
}