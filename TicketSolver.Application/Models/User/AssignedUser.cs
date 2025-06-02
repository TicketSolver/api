namespace TicketSolver.Application.Models.User;

public class AssignedUser
{
    public int Id { get; set; }
    public UserDto User { get; set; }
}

public record UserDto(string Id, string? Email, string? Name);