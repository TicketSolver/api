namespace TicketSolver.Application.Models.Chat;


public class ChatActivitySummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalChats { get; set; }
    public int TotalMessages { get; set; }
    public int ActiveChats { get; set; }
    public Dictionary<string, int> MessagesByDay { get; set; } = [];
    public Dictionary<string, int> MessagesBySenderType { get; set; } = [];
}