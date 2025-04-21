using TicketSolver.Api.Models.Enums;

namespace TicketSolver.Api.Models;

public class User
{
    public int Id { get; set; }
    public string AspNetUserId { get; set; }
    public string FullName { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; }
}