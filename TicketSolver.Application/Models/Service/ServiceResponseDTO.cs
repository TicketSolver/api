namespace TicketSolver.Application.Models.Service;

public class ServiceResponseDTO
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string RequestedByName { get; set; } = null!;
    public string? AssignedTechName { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? ServicePeriodName { get; set; }
    public short Status { get; set; }
    public string StatusName { get; set; } = null!;
    public string? ServiceReport { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public AddressDTO Address { get; set; } = null!;
    public List<AvailableSlotResponseDTO> AvailableSlots { get; set; } = [];
}