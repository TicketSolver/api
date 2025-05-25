namespace TicketSolver.Domain.Models.Ticket;

public class UserShort
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public short UserTypeId { get; set; }
}