namespace TicketSolver.Application.Models.Service;

public class ServiceRequestInputDTO
{
    public int TicketId { get; set; }
    public AddressDTO Address { get; set; } = null!;
    public string? Comments { get; set; }
}