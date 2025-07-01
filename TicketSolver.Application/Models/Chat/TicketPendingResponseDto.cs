namespace TicketSolver.Application.Models.Chat
{
    public class TicketPendingResponseDto
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public string TicketDescription { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string LastUserMessage { get; set; } = string.Empty;
        public DateTime LastUserMessageAt { get; set; }
        public int UnreadUserMessages { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTechnician { get; set; } = string.Empty;
    }
}