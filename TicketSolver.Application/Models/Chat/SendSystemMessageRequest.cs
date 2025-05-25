namespace TicketSolver.Application.Models.Chat;

public class SendSystemMessageRequestDto
{
    public int TicketId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
}