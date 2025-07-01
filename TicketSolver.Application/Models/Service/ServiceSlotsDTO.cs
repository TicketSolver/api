namespace TicketSolver.Application.Models.Service;


public class ServiceSlotsDTO
{
    public int ServiceId { get; set; }
    public List<AvailableSlotDTO> Slots { get; set; } = [];
}