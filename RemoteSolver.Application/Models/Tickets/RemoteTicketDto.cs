using TicketSolver.Application.Models;

namespace RemoteSolver.Application.Models.Tickets;

public class RemoteTicketDto : TicketDTO
{
    public string? RemoteAccessLink { get; set; }
    public string? AccessCredentials { get; set; }
    public string? OperatingSystem { get; set; }
}
