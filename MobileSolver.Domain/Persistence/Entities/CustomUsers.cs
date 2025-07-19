using System.ComponentModel.DataAnnotations.Schema;
using TicketSolver.Domain.Persistence.Tables.User;

namespace MobileSolver.Domain.Persistence.Entities;

[Table("AspNetUsers")]
public class CustomUsers : Users
{
    public string PhoneNumber { get; set; }
    public string Github { get; set; }
}