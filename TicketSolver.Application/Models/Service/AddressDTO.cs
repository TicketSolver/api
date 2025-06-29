namespace TicketSolver.Application.Models.Service;

public class AddressDTO
{
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = "Brasil";
    public string? Complement { get; set; }
    public string? Reference { get; set; }
}