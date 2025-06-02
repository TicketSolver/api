namespace TicketSolver.Application.Models.User;

public class TechnicianCounters
{
    public int CurrentlyWorking { get; set; }
    public int SolvedToday { get; set; }
    public int HighPriority { get; set; }
    public int Critical { get; set; }
}