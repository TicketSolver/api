using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TicketSolver.Domain.Enums;

namespace TicketSolver.Domain.Persistence.Tables.User;

public class Users
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public eUserType EUserType { get; set; }
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdentityUserId")] public IdentityUser IdentityUser { get; set; }
}