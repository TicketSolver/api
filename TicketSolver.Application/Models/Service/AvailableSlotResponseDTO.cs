namespace TicketSolver.Application.Models.Service;

public class AvailableSlotResponseDTO
{
    public int Id { get; set; }
    public DateTime AvailableDate { get; set; }
    public short Period { get; set; }
    public string PeriodName { get; set; } = null!;
    public bool IsSelected { get; set; }
}