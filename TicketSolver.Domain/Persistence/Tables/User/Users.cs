using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TicketSolver.Domain.Persistence.Tables.Defs;
using TicketSolver.Domain.Persistence.Tables.Tenant;
using TicketSolver.Domain.Persistence.Tables.Ticket;

namespace TicketSolver.Domain.Persistence.Tables.User;

public class Users : IdentityUser
{
    public string FullName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public short DefUserTypeId { get; set; }
    [ForeignKey("DefUserTypeId")] public DefUserTypes DefUserType { get; set; }
    
    public int TenantId { get; set; }
    [ForeignKey("TenantId")] public Tenants Tenant { get; set; }
    public ICollection<TicketUsers> TicketUsers { get; set; } = [];
}