using System.ComponentModel.DataAnnotations;
using TicketSolver.Domain.Persistence.Tables.User;

namespace TicketSolver.Domain.Persistence.Tables.Tenant;

public class Tenants : EntityDates
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }
    public Guid AdminKey { get; set; }
    public Guid PublicKey { get; set; }
    public bool IsConfigured { get; set; }

    public ICollection<Users> Users { get; set; } = [];
}