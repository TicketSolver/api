namespace TicketSolver.Application.Models.Service;

public class ServiceRequestDTO
{
    public int TicketId { get; set; }
    public string RequestedById { get; set; } = null!;
    public AddressDTO Address { get; set; } = null!;
    public string? Comments { get; set; }
}