namespace TicketSolver.Domain.Persistence.Tables.Service;

public class ServiceAddress : EntityDates
{
    public int Id { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = "Brasil";
    public string? Complement { get; set; }
    public string? Reference { get; set; }
}