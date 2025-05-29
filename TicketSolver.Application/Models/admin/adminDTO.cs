namespace TicketSolver.Application.Models.admin;

public class TenantOverviewStatsDto
{
    public int TotalTickets { get; set; }
    public int ActiveTickets { get; set; }
    public string AvgResolutionTime { get; set; } = null!;
    public int ActiveTechnicians { get; set; }
}


public class RecentTicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? TechnicianName { get; set; }
}

