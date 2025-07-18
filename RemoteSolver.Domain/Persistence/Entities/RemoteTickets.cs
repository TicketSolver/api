using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace RemoteSolver.Domain.Persistence.Entities;

[Table("Tickets")]
public class RemoteTickets : Tickets
{
    public string? RemoteAccessLink { get; set; }
    public string? AccessCredentials { get; set; }
    public string? OperatingSystem { get; set; }
}
